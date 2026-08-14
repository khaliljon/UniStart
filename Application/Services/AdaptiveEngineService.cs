using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class AdaptiveEngineService : IAdaptiveEngineService
{
    private readonly UniStartDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

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

    public async Task<QuestionDto?> GetNextQuestionAsync(int userId, string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null)
    {
        var answeredQuestionIds = await _context.UserAnswers
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.QuestionId)
            .Distinct()
            .ToListAsync();

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

            var recyclableIds = allRecyclable.Select(q => q.Id).ToList();
            var rawAnswers = await _context.UserAnswers
                .Where(ua => ua.UserId == userId && recyclableIds.Contains(ua.QuestionId))
                .Include(ua => ua.AnswerOption)
                .OrderByDescending(ua => ua.AnsweredAt)
                .ToListAsync();

            var lastAnswers = rawAnswers
                .GroupBy(ua => ua.QuestionId)
                .Select(g => {
                    var ordered = g.ToList();
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

            availableQuestions = allRecyclable
                .Where(q => !lookup.TryGetValue(q.Id, out var a) || a.CorrectStreak < 2)
                .OrderBy(q => lookup.TryGetValue(q.Id, out var a) && a.WasCorrect ? 1 : 0)
                .ThenBy(q => lookup.TryGetValue(q.Id, out var a) ? a.LastAnswered : DateTime.MinValue)
                .ToList();

            if (availableQuestions.Count == 0)
            {
                availableQuestions = allRecyclable
                    .OrderBy(q => lookup.TryGetValue(q.Id, out var a) ? a.LastAnswered : DateTime.MinValue)
                    .ToList();
            }

            var candidateCount = Math.Min(availableQuestions.Count, Math.Max(5, availableQuestions.Count / 2));
            availableQuestions = availableQuestions.Take(candidateCount).ToList();
        }

        var theta = await GetUserThetaAsync(userId);

        var selected = IrtMath.SelectNextItemRandomized(theta, availableQuestions, topFraction: 0.7);

        selected ??= availableQuestions[Random.Shared.Next(availableQuestions.Count)];

        return MapToQuestionDto(selected);
    }

    public async Task<AnswerResultDto> ProcessAnswerAsync(int userId, SubmitAnswerDto answer)
    {
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

        var sectionId = question.Topic.SectionId
            ?? throw new InvalidOperationException("Question's topic has no section; cannot update ability.");
        var (newLevel, change, theta, thetaSE) = await UpdateSkillLevelAsync(userId, sectionId, isCorrect);

        question.DifficultyParam = IrtMath.UpdateDifficultyOnline(
            question.DifficultyParam, question.DiscriminationParam, question.GuessParam,
            theta, isCorrect, question.ResponseCount);
        question.ResponseCount++;
        if (question.ResponseCount >= IrtMath.CalibrationThreshold)
            question.IsCalibrated = true;

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

    public async Task<(int newLevel, int change, double theta, double thetaSE)> UpdateSkillLevelAsync(int userId, int sectionId, bool isCorrect)
    {
        var profile = await _context.UserSkillProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SectionId == sectionId);

        if (profile == null)
        {
            profile = new UserSkillProfile
            {
                UserId = userId,
                SectionId = sectionId,
                Level = 50,
                Theta = 0.0,
                ThetaSE = 1.0
            };
            _context.UserSkillProfiles.Add(profile);
        }

        var oldLevel = profile.Level;

        var skillAnswers = await _context.UserAnswers
            .Include(ua => ua.Question)
                .ThenInclude(q => q.Topic)
            .Include(ua => ua.AnswerOption)
            .Where(ua => ua.UserId == userId && ua.Question.Topic.SectionId == sectionId)
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

        var responses = skillAnswers
            .Select(ua => (ua.Question, ua.AnswerOption.IsCorrect))
            .ToList();

        var (theta, se) = IrtMath.EstimateAbilityEAP(responses, priorMean: 0.0, priorSD: 1.5);

        profile.Theta = theta;
        profile.ThetaSE = se;
        profile.Level = IrtMath.ThetaToLevel(theta);
        profile.LastUpdated = DateTime.UtcNow;

        return (profile.Level, profile.Level - oldLevel, profile.Theta, profile.ThetaSE);
    }

    public async Task<UserSkillProfileDto?> GetUserSkillProfileAsync(int userId, int sectionId)
    {
        var profile = await _context.UserSkillProfiles
            .Include(p => p.Section)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SectionId == sectionId);

        if (profile == null) return null;

        return MapToSkillProfileDto(profile);
    }

    public async Task<IEnumerable<UserSkillProfileDto>> GetUserSkillProfilesAsync(int userId)
    {
        var profiles = await _context.UserSkillProfiles
            .Include(p => p.Section)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return profiles.Select(MapToSkillProfileDto);
    }

    private static UserSkillProfileDto MapToSkillProfileDto(UserSkillProfile p)
    {
        var confLow = IrtMath.ThetaToLevel(p.Theta - 1.96 * p.ThetaSE);
        var confHigh = IrtMath.ThetaToLevel(p.Theta + 1.96 * p.ThetaSE);
        return new UserSkillProfileDto(
            p.SectionId,
            p.Section.Name,
            p.Section.Name,
            p.Level,
            p.LastUpdated,
            p.Theta,
            p.ThetaSE,
            confLow,
            confHigh
        );
    }

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
            HasHint: !string.IsNullOrEmpty(question.Hint),
            ImageUrl: question.ImageUrl
        );
    }

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

    public async Task<int> GetTopicMasteryAsync(int userId, string[] examTypeCodes, int? topicId = null)
    {
        if (!topicId.HasValue) return 0;

        var totalQuestions = await GetTotalQuestionsCountAsync(examTypeCodes, topicId: topicId);
        if (totalQuestions == 0) return 0;

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

        var now = DateTime.UtcNow;
        var questionsWithPriority = incorrectAnswers
            .GroupBy(ua => ua.QuestionId)
            .Select(g =>
            {
                var question = g.First().Question;
                var lastAnswer = g.MaxBy(ua => ua.AnsweredAt)!;
                var daysSinceLastReview = (now - lastAnswer.AnsweredAt).TotalDays;
                var incorrectCount = g.Count();
                
                var stability = IrtMath.CalculateStability(0, baseStability: 0.5, factor: 1.0);
                var retention = IrtMath.RetentionProbability(daysSinceLastReview, stability);
                
                var priority = retention - (incorrectCount * 0.1);
                
                return (question, priority);
            })
            .OrderBy(x => x.priority)
            .Select(x => MapToQuestionDto(x.question));

        return questionsWithPriority;
    }

    public async Task<IEnumerable<TopicProgressDto>> GetTopicsWithProgressAsync(int userId, string[]? examTypeCodes = null, int[]? sectionIds = null)
    {
        var topicsQuery = _context.Topics
            .Include(t => t.Section)
            .Include(t => t.Questions)
            .Include(t => t.Lessons)
            .AsQueryable();

        if (sectionIds != null && sectionIds.Length > 0)
        {
            topicsQuery = topicsQuery.Where(t => t.SectionId != null && sectionIds.Contains(t.SectionId.Value));
        }
        else if (examTypeCodes != null && examTypeCodes.Length > 0)
        {
            topicsQuery = topicsQuery.Where(t => t.Section != null && examTypeCodes.Contains(t.Section.ExamTypeCode));
        }

        var topics = await topicsQuery.ToListAsync();

        var skillProfiles = await _context.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SectionId);

        var userAnswers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Include(ua => ua.Question)
                .ThenInclude(q => q.Topic)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var result = topics.Select(topic =>
        {
            var topicAnswers = userAnswers.Where(ua => ua.Question.TopicId == topic.Id).ToList();
            
            var uniqueCorrectQuestions = topicAnswers
                .Where(ua => ua.AnswerOption.IsCorrect)
                .Select(ua => ua.QuestionId)
                .Distinct()
                .Count();
            
            var correctAttempts = topicAnswers.Count(ua => ua.AnswerOption.IsCorrect);
            var incorrectAttempts = topicAnswers.Count(ua => !ua.AnswerOption.IsCorrect);
            
            var totalQuestions = topic.Questions.Count;
            
            var mastery = totalQuestions > 0 
                ? Math.Min((double)uniqueCorrectQuestions / totalQuestions * 100, 100.0)
                : 0;

            var irtLevel = topic.SectionId is int tsecId && skillProfiles.TryGetValue(tsecId, out var sp)
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
