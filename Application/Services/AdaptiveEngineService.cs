using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Adaptive Engine v2 — IRT-based (3PL) with EAP ability estimation and CAT question selection.
/// 
/// Key changes from v1:
/// - Question selection: CAT (maximum Fisher information) instead of difficulty-threshold matching
/// - Skill update: Bayesian EAP estimation of θ instead of linear +5/−3
/// - Display level: θ mapped to 0–100 via logistic function
/// - Forgetting curve: Ebbinghaus decay applied to spaced repetition priority
/// - Topic dependencies: prerequisite-aware question ordering
/// </summary>
public class AdaptiveEngineService : IAdaptiveEngineService
{
    private readonly UniStartDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    // Legacy thresholds kept for GetDifficultyForSkillLevel (used externally)
    private const int EasyThreshold = 40;
    private const int MediumThreshold = 70;

    public AdaptiveEngineService(UniStartDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public QuestionDifficulty GetDifficultyForSkillLevel(int skillLevel)
    {
        if (skillLevel < EasyThreshold) return QuestionDifficulty.Easy;
        if (skillLevel < MediumThreshold) return QuestionDifficulty.Medium;
        return QuestionDifficulty.Hard;
    }

    // ─── CAT Question Selection (IRT-based) ──────────────────────

    /// <summary>
    /// Selects next question using Computerized Adaptive Testing (CAT).
    /// Uses maximum Fisher information criterion with randomized top-fraction selection.
    /// </summary>
    public async Task<QuestionDto?> GetNextQuestionAsync(int userId, string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null)
    {
        // Get user's answered question IDs to avoid repetition
        var answeredQuestionIds = await _context.UserAnswers
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.QuestionId)
            .Distinct()
            .ToListAsync();

        // Load available questions with their IRT parameters
        var questionsQuery = _context.Questions
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section)
            .Include(q => q.AnswerOptions)
            .Where(q => !answeredQuestionIds.Contains(q.Id));

        if (examTypeCodes.Length > 0)
        {
            questionsQuery = questionsQuery.Where(q =>
                q.Topic.Section != null &&
                examTypeCodes.Contains(q.Topic.Section.ExamTypeCode));
        }

        // Filter by multiple section IDs (preferred) or single sectionId (backward compat)
        if (sectionIds is { Length: > 0 })
        {
            questionsQuery = questionsQuery.Where(q => q.Topic.SectionId != null && sectionIds.Contains(q.Topic.SectionId.Value));
        }
        else if (sectionId.HasValue)
        {
            questionsQuery = questionsQuery.Where(q => q.Topic.SectionId == sectionId.Value);
        }

        if (topicId.HasValue)
        {
            questionsQuery = questionsQuery.Where(q => q.TopicId == topicId.Value);
        }

        var availableQuestions = await questionsQuery.ToListAsync();
        
        // If no unanswered questions remain, recycle previously answered ones
        // Only recycle questions that still need work (not mastered)
        if (availableQuestions.Count == 0)
        {
            var recycleQuery = _context.Questions
                .Include(q => q.Topic)
                    .ThenInclude(t => t.Section)
                .Include(q => q.AnswerOptions)
                .Where(q => answeredQuestionIds.Contains(q.Id));

            if (examTypeCodes.Length > 0)
                recycleQuery = recycleQuery.Where(q => q.Topic.Section != null && examTypeCodes.Contains(q.Topic.Section.ExamTypeCode));
            if (sectionIds is { Length: > 0 })
                recycleQuery = recycleQuery.Where(q => q.Topic.SectionId != null && sectionIds.Contains(q.Topic.SectionId.Value));
            else if (sectionId.HasValue)
                recycleQuery = recycleQuery.Where(q => q.Topic.SectionId == sectionId.Value);
            if (topicId.HasValue)
                recycleQuery = recycleQuery.Where(q => q.TopicId == topicId.Value);

            var allRecyclable = await recycleQuery.ToListAsync();
            if (allRecyclable.Count == 0) return null;

            // Get answer history for each question (fetch raw, compute streaks in memory)
            var recyclableIds = allRecyclable.Select(q => q.Id).ToList();
            var rawAnswers = await _context.UserAnswers
                .Where(ua => ua.UserId == userId && recyclableIds.Contains(ua.QuestionId))
                .Include(ua => ua.AnswerOption)
                .OrderByDescending(ua => ua.AnsweredAt)
                .ToListAsync();

            var lastAnswers = rawAnswers
                .GroupBy(ua => ua.QuestionId)
                .Select(g => {
                    var ordered = g.ToList(); // already sorted desc by AnsweredAt
                    var streak = 0;
                    foreach (var ua in ordered)
                    {
                        if (ua.AnswerOption?.IsCorrect == true) streak++;
                        else break;
                    }
                    return new {
                        QuestionId = g.Key,
                        LastAnswered = ordered.First().AnsweredAt,
                        WasCorrect = ordered.First().AnswerOption?.IsCorrect == true,
                        CorrectStreak = streak
                    };
                })
                .ToList();

            var lookup = lastAnswers.ToDictionary(a => a.QuestionId);

            // Filter out questions with 2+ consecutive correct answers (mastered)
            availableQuestions = allRecyclable
                .Where(q => !lookup.TryGetValue(q.Id, out var a) || a.CorrectStreak < 2)
                .OrderBy(q => lookup.TryGetValue(q.Id, out var a) && a.WasCorrect ? 1 : 0)
                .ThenBy(q => lookup.TryGetValue(q.Id, out var a) ? a.LastAnswered : DateTime.MinValue)
                .ToList();

            // If all questions are mastered, allow free practice over all questions
            // (user can keep practicing even after mastery — just recycle everything)
            if (availableQuestions.Count == 0)
            {
                availableQuestions = allRecyclable
                    .OrderBy(q => lookup.TryGetValue(q.Id, out var a) ? a.LastAnswered : DateTime.MinValue)
                    .ToList();
            }

            // Take top candidates for CAT selection
            var candidateCount = Math.Min(availableQuestions.Count, Math.Max(5, availableQuestions.Count / 2));
            availableQuestions = availableQuestions.Take(candidateCount).ToList();
        }

        // Get user's current θ estimate (average across skills relevant to these exams)
        var theta = await GetUserThetaAsync(userId);

        // CAT: select question with maximum information at current θ, with randomized top fraction
        var selected = IrtMath.SelectNextItemRandomized(theta, availableQuestions, topFraction: 0.7);

        // Fallback to random if CAT fails
        selected ??= availableQuestions[Random.Shared.Next(availableQuestions.Count)];

        return MapToQuestionDto(selected);
    }

    // ─── IRT-based Answer Processing ─────────────────────────────

    /// <summary>
    /// Processes answer and updates skill using IRT EAP estimation.
    /// </summary>
    public async Task<AnswerResultDto> ProcessAnswerAsync(int userId, SubmitAnswerDto answer)
    {
        // Validate TestSession ownership if provided
        if (answer.TestSessionId.HasValue)
        {
            var session = await _context.TestSessions
                .FirstOrDefaultAsync(s => s.Id == answer.TestSessionId.Value);
            if (session == null)
                throw new ArgumentException("Test session not found");
            if (session.UserId != userId)
                throw new ArgumentException("Test session does not belong to this user");
            if (session.CompletedAt != null)
                throw new ArgumentException("Test session is already completed");
        }

        var question = await _context.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.Topic)
            .FirstOrDefaultAsync(q => q.Id == answer.QuestionId)
            ?? throw new ArgumentException("Question not found");

        var selectedOption = question.AnswerOptions.FirstOrDefault(o => o.Id == answer.AnswerOptionId)
            ?? throw new ArgumentException("Answer option not found");

        // Prevent rapid duplicate submits of the exact same answer (double-click protection only)
        var duplicateCutoff = DateTime.UtcNow.AddSeconds(-1);
        var recentDuplicate = await _context.UserAnswers
            .AnyAsync(ua => ua.UserId == userId 
                         && ua.QuestionId == answer.QuestionId 
                         && ua.AnswerOptionId == answer.AnswerOptionId
                         && ua.AnsweredAt > duplicateCutoff);
        if (recentDuplicate)
            throw new ArgumentException("Вы уже ответили на этот вопрос");

        var correctOption = question.AnswerOptions.First(o => o.IsCorrect);
        var isCorrect = selectedOption.IsCorrect;

        // Record the answer
        var userAnswer = new UserAnswer
        {
            UserId = userId,
            QuestionId = question.Id,
            AnswerOptionId = answer.AnswerOptionId,
            AnsweredAt = DateTime.UtcNow,
            TimeSpentSeconds = answer.TimeSpentSeconds,
            TestSessionId = answer.TestSessionId
        };
        _context.UserAnswers.Add(userAnswer);

        // Update skill using IRT EAP estimation
        var (newLevel, change, theta, thetaSE) = await UpdateSkillLevelAsync(userId, question.Topic.SkillId, isCorrect);

        var confLow = IrtMath.ThetaToLevel(theta - 1.96 * thetaSE);
        var confHigh = IrtMath.ThetaToLevel(theta + 1.96 * thetaSE);

        await _unitOfWork.SaveChangesAsync();

        return new AnswerResultDto(
            isCorrect,
            correctOption.Id,
            correctOption.Text,
            question.Explanation,
            newLevel,
            change,
            theta,
            thetaSE,
            confLow,
            confHigh
        );
    }

    // ─── IRT Skill Update (EAP Estimation) ───────────────────────

    /// <summary>
    /// Updates skill level using Bayesian EAP estimation of θ.
    /// Replays all answers for this skill and recomputes θ from scratch.
    /// </summary>
    public async Task<(int newLevel, int change, double theta, double thetaSE)> UpdateSkillLevelAsync(int userId, int skillId, bool isCorrect)
    {
        var profile = await _context.UserSkillProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SkillId == skillId);

        if (profile == null)
        {
            profile = new UserSkillProfile
            {
                UserId = userId,
                SkillId = skillId,
                Level = 50,
                Theta = 0.0,
                ThetaSE = 1.0
            };
            _context.UserSkillProfiles.Add(profile);
        }

        var oldLevel = profile.Level;

        // Get all answers for this skill to re-estimate θ
        var skillAnswers = await _context.UserAnswers
            .Include(ua => ua.Question)
                .ThenInclude(q => q.Topic)
            .Include(ua => ua.AnswerOption)
            .Where(ua => ua.UserId == userId && ua.Question.Topic.SkillId == skillId)
            .OrderBy(ua => ua.AnsweredAt)
            .ToListAsync();

        if (skillAnswers.Count == 0)
        {
            profile.Level = 50;
            profile.Theta = 0.0;
            profile.ThetaSE = 1.0;
            profile.LastUpdated = DateTime.UtcNow;
            return (profile.Level, 0, profile.Theta, profile.ThetaSE);
        }

        // Build response vector for EAP
        var responses = skillAnswers
            .Select(ua => (ua.Question, ua.AnswerOption.IsCorrect))
            .ToList();

        // Use current θ as prior mean for Bayesian continuity
        var (theta, se) = IrtMath.EstimateAbilityEAP(responses, priorMean: 0.0, priorSD: 1.5);

        profile.Theta = theta;
        profile.ThetaSE = se;
        profile.Level = IrtMath.ThetaToLevel(theta);
        profile.LastUpdated = DateTime.UtcNow;

        return (profile.Level, profile.Level - oldLevel, profile.Theta, profile.ThetaSE);
    }

    public async Task<UserSkillProfileDto?> GetUserSkillProfileAsync(int userId, int skillId)
    {
        var profile = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SkillId == skillId);

        if (profile == null) return null;

        return MapToSkillProfileDto(profile);
    }

    public async Task<IEnumerable<UserSkillProfileDto>> GetUserSkillProfilesAsync(int userId)
    {
        var profiles = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return profiles.Select(MapToSkillProfileDto);
    }

    private static UserSkillProfileDto MapToSkillProfileDto(UserSkillProfile p)
    {
        var confLow = IrtMath.ThetaToLevel(p.Theta - 1.96 * p.ThetaSE);
        var confHigh = IrtMath.ThetaToLevel(p.Theta + 1.96 * p.ThetaSE);
        return new UserSkillProfileDto(
            p.SkillId,
            p.Skill.Name,
            p.Skill.Code,
            p.Level,
            p.LastUpdated,
            p.Theta,
            p.ThetaSE,
            confLow,
            confHigh
        );
    }

    // ─── IRT Helpers ─────────────────────────────────────────────

    /// <summary>
    /// Gets user's average θ across all skills for CAT question selection.
    /// </summary>
    private async Task<double> GetUserThetaAsync(int userId)
    {
        var avgTheta = await _context.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .AverageAsync(p => (double?)p.Theta);

        return avgTheta ?? 0.0;
    }

    private static QuestionDto MapToQuestionDto(Question question)
    {
        return new QuestionDto(
            question.Id,
            question.Text,
            question.Difficulty.ToString(),
            question.TopicId,
            question.Topic.Name,
            question.AnswerOptions.Select(o => new AnswerOptionDto(o.Id, o.Text)),
            HasHint: !string.IsNullOrEmpty(question.Hint)
        );
    }

    /// <summary>
    /// Resets user's test progress — clears answers and resets skill profiles to defaults.
    /// </summary>
    public async Task ResetUserProgressAsync(int userId)
    {
        var userAnswers = await _context.UserAnswers
            .Where(ua => ua.UserId == userId)
            .ToListAsync();
        _context.UserAnswers.RemoveRange(userAnswers);

        var skillProfiles = await _context.UserSkillProfiles
            .Where(sp => sp.UserId == userId)
            .ToListAsync();
        foreach (var profile in skillProfiles)
        {
            profile.Level = 50;
            profile.Theta = 0.0;
            profile.ThetaSE = 1.0;
            profile.LastUpdated = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Gets total question count for selected exams, optionally filtered by section/topic
    /// </summary>
    public async Task<int> GetTotalQuestionsCountAsync(string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null)
    {
        var query = _context.Questions
            .Where(q => q.Topic.Section != null && examTypeCodes.Contains(q.Topic.Section.ExamTypeCode));

        if (topicId.HasValue)
            query = query.Where(q => q.TopicId == topicId.Value);
        else if (sectionIds is { Length: > 0 })
            query = query.Where(q => q.Topic.SectionId != null && sectionIds.Contains(q.Topic.SectionId.Value));
        else if (sectionId.HasValue)
            query = query.Where(q => q.Topic.SectionId == sectionId.Value);

        return await query.CountAsync();
    }

    /// <summary>
    /// Gets count of answered questions for user in selected exams, optionally filtered by section/topic
    /// </summary>
    public async Task<int> GetAnsweredQuestionsCountAsync(int userId, string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null)
    {
        var query = _context.UserAnswers
            .Where(ua => ua.UserId == userId && ua.Question.Topic.Section != null && examTypeCodes.Contains(ua.Question.Topic.Section.ExamTypeCode));

        if (topicId.HasValue)
            query = query.Where(ua => ua.Question.TopicId == topicId.Value);
        else if (sectionIds is { Length: > 0 })
            query = query.Where(ua => ua.Question.Topic.SectionId != null && sectionIds.Contains(ua.Question.Topic.SectionId.Value));
        else if (sectionId.HasValue)
            query = query.Where(ua => ua.Question.Topic.SectionId == sectionId.Value);

        return await query.CountAsync();
    }

    /// <summary>
    /// Calculates mastery percentage for a topic based on the last answer to each question.
    /// Mastery = (questions with last answer correct) / (total questions in topic) * 100
    /// Returns 0 if no questions answered yet.
    /// </summary>
    public async Task<int> GetTopicMasteryAsync(int userId, string[] examTypeCodes, int? topicId = null)
    {
        if (!topicId.HasValue) return 0;

        var totalQuestions = await GetTotalQuestionsCountAsync(examTypeCodes, topicId: topicId);
        if (totalQuestions == 0) return 0;

        // Get the last answer for each question in this topic
        var lastAnswerPerQuestion = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Include(ua => ua.Question)
            .Where(ua => ua.UserId == userId && ua.Question.TopicId == topicId.Value)
            .GroupBy(ua => ua.QuestionId)
            .Select(g => new {
                QuestionId = g.Key,
                WasCorrect = g.OrderByDescending(ua => ua.AnsweredAt).First().AnswerOption!.IsCorrect
            })
            .ToListAsync();

        if (lastAnswerPerQuestion.Count == 0) return 0;

        var correctCount = lastAnswerPerQuestion.Count(a => a.WasCorrect);
        return (int)Math.Round((double)correctCount / totalQuestions * 100);
    }

    /// <summary>
    /// Gets questions for spaced repetition review, prioritized by Ebbinghaus forgetting curve.
    /// Questions with lower retention probability appear first (most urgently need review).
    /// </summary>
    public async Task<IEnumerable<QuestionDto>> GetIncorrectlyAnsweredQuestionsAsync(int userId, string[]? examTypeCodes = null)
    {
        var query = _context.UserAnswers
            .Include(ua => ua.Question)
                .ThenInclude(q => q.Topic)
                    .ThenInclude(t => t.Section)
            .Include(ua => ua.Question)
                .ThenInclude(q => q.AnswerOptions)
            .Include(ua => ua.AnswerOption)
            .Where(ua => ua.UserId == userId && !ua.AnswerOption.IsCorrect);

        if (examTypeCodes != null && examTypeCodes.Length > 0)
        {
            query = query.Where(ua => ua.Question.Topic.Section != null && examTypeCodes.Contains(ua.Question.Topic.Section.ExamTypeCode));
        }

        var incorrectAnswers = await query.ToListAsync();

        // Group by question, calculate forgetting-curve priority
        var now = DateTime.UtcNow;
        var questionsWithPriority = incorrectAnswers
            .GroupBy(ua => ua.QuestionId)
            .Select(g =>
            {
                var question = g.First().Question;
                var lastAnswer = g.MaxBy(ua => ua.AnsweredAt)!;
                var daysSinceLastReview = (now - lastAnswer.AnsweredAt).TotalDays;
                var incorrectCount = g.Count();
                
                // Stability decreases with more errors; base stability = 1 day for never-correct items
                var stability = IrtMath.CalculateStability(0, baseStability: 0.5, factor: 1.0);
                var retention = IrtMath.RetentionProbability(daysSinceLastReview, stability);
                
                // Priority: lower retention + more errors = higher priority (lower value = show first)
                var priority = retention - (incorrectCount * 0.1);
                
                return (question, priority);
            })
            .OrderBy(x => x.priority) // lowest priority first = most needs review
            .Select(x => MapToQuestionDto(x.question));

        return questionsWithPriority;
    }

    /// <summary>
    /// Gets topics with user progress statistics
    /// </summary>
    public async Task<IEnumerable<TopicProgressDto>> GetTopicsWithProgressAsync(int userId, string[]? examTypeCodes = null)
    {
        var topicsQuery = _context.Topics
            .Include(t => t.Section)
            .Include(t => t.Questions)
            .Include(t => t.Lessons)
            .AsQueryable();

        if (examTypeCodes != null && examTypeCodes.Length > 0)
        {
            topicsQuery = topicsQuery.Where(t => t.Section != null && examTypeCodes.Contains(t.Section.ExamTypeCode));
        }

        var topics = await topicsQuery.ToListAsync();

        // Fetch IRT skill profiles for the user
        var skillProfiles = await _context.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SkillId);

        var userAnswers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Include(ua => ua.Question)
                .ThenInclude(q => q.Topic)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var result = topics.Select(topic =>
        {
            var topicAnswers = userAnswers.Where(ua => ua.Question.TopicId == topic.Id).ToList();
            
            // Count unique questions answered correctly (at least once)
            var uniqueCorrectQuestions = topicAnswers
                .Where(ua => ua.AnswerOption.IsCorrect)
                .Select(ua => ua.QuestionId)
                .Distinct()
                .Count();
            
            // Total attempts for display
            var correctAttempts = topicAnswers.Count(ua => ua.AnswerOption.IsCorrect);
            var incorrectAttempts = topicAnswers.Count(ua => !ua.AnswerOption.IsCorrect);
            
            var totalQuestions = topic.Questions.Count;
            
            // Mastery based on unique correct questions (capped at 100%)
            var mastery = totalQuestions > 0 
                ? Math.Min((double)uniqueCorrectQuestions / totalQuestions * 100, 100.0)
                : 0;

            // IRT level from skill profile (same formula as WhatIf)
            var irtLevel = skillProfiles.TryGetValue(topic.SkillId, out var sp)
                ? IrtMath.ThetaToLevel(sp.Theta)
                : 50;

            return new TopicProgressDto(
                topic.Id,
                topic.Name,
                totalQuestions,
                correctAttempts,
                incorrectAttempts,
                Math.Round(mastery, 1),
                LessonCount: topic.Lessons.Count,
                HasVideoLessons: topic.Lessons.Any(l => l.VideoUrl != null),
                IrtLevel: irtLevel,
                SectionId: topic.SectionId,
                SectionName: topic.Section?.Name,
                ExamTypeCode: topic.Section?.ExamTypeCode
            );
        });

        return result.OrderBy(t => t.MasteryPercentage);
    }

    /// <summary>
    /// Gets all questions for a specific topic
    /// </summary>
    public async Task<IEnumerable<QuestionDto>> GetQuestionsByTopicAsync(int topicId)
    {
        var questions = await _context.Questions
            .Include(q => q.Topic)
            .Include(q => q.AnswerOptions)
            .Where(q => q.TopicId == topicId)
            .ToListAsync();

        return questions.Select(MapToQuestionDto);
    }
}
