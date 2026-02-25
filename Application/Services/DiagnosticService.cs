using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class DiagnosticService : IDiagnosticService
{
    private readonly UniStartDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdaptiveEngineService _adaptiveEngine;
    private const int DiagnosticQuestionCount = 10;
    private const string DiagnosticMode = "diagnostic";

    public DiagnosticService(
        UniStartDbContext context,
        IUnitOfWork unitOfWork,
        IAdaptiveEngineService adaptiveEngine)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _adaptiveEngine = adaptiveEngine;
    }

    public async Task<DiagnosticSessionDto> StartAsync(int userId, string examTypeCode)
    {
        var examType = await _context.ExamTypes
            .Include(e => e.Sections)
            .FirstOrDefaultAsync(e => e.Code == examTypeCode)
            ?? throw new InvalidOperationException($"Exam type '{examTypeCode}' not found");

        // Abandon any existing in-progress diagnostic sessions
        var existingSessions = await _context.TestSessions
            .Where(s => s.UserId == userId && s.Mode == DiagnosticMode && s.CompletedAt == null)
            .ToListAsync();
        foreach (var es in existingSessions)
        {
            es.CompletedAt = DateTime.UtcNow;
            es.Score = 0;
        }

        // Select questions: spread across sections, mix of difficulties
        var questions = await SelectDiagnosticQuestionsAsync(examTypeCode, userId);

        // Create session
        var session = new TestSession
        {
            UserId = userId,
            ExamTypeCode = examTypeCode,
            Mode = DiagnosticMode,
            StartedAt = DateTime.UtcNow,
            TotalQuestions = questions.Count
        };
        _context.TestSessions.Add(session);
        await _unitOfWork.SaveChangesAsync();

        // Pre-create placeholder answers to track question order
        for (int i = 0; i < questions.Count; i++)
        {
            _context.UserAnswers.Add(new UserAnswer
            {
                UserId = userId,
                QuestionId = questions[i].Id,
                AnswerOptionId = questions[i].AnswerOptions.First().Id, // placeholder
                AnsweredAt = DateTime.UtcNow.AddYears(10), // far future = not yet answered
                TestSessionId = session.Id,
                TimeSpentSeconds = -1 // sentinel: not yet answered
            });
        }
        await _unitOfWork.SaveChangesAsync();

        return new DiagnosticSessionDto(
            session.Id,
            examTypeCode,
            examType.Name,
            questions.Count,
            0,
            false
        );
    }

    public async Task<DiagnosticQuestionDto?> GetCurrentQuestionAsync(int userId, int sessionId)
    {
        var session = await _context.TestSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.Mode == DiagnosticMode)
            ?? throw new InvalidOperationException("Diagnostic session not found");

        if (session.CompletedAt != null)
            return null; // already completed

        // Find first unanswered question (TimeSpentSeconds == -1 is sentinel)
        var answers = await _context.UserAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.Topic)
                    .ThenInclude(t => t.Section)
            .Include(a => a.Question)
                .ThenInclude(q => q.AnswerOptions)
            .Where(a => a.TestSessionId == sessionId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        var currentIndex = answers.FindIndex(a => a.TimeSpentSeconds == -1);
        if (currentIndex == -1)
            return null; // all answered

        var answer = answers[currentIndex];
        var question = answer.Question;

        return new DiagnosticQuestionDto(
            currentIndex,
            answers.Count,
            question.Id,
            question.Text,
            question.Difficulty.ToString(),
            question.Topic.Name,
            question.Topic.Section?.Name ?? "",
            question.AnswerOptions.Select(o => new AnswerOptionDto(o.Id, o.Text))
        );
    }

    public async Task<DiagnosticAnswerResultDto> SubmitAnswerAsync(int userId, DiagnosticAnswerDto dto)
    {
        var session = await _context.TestSessions
            .FirstOrDefaultAsync(s => s.Id == dto.SessionId && s.UserId == userId && s.Mode == DiagnosticMode)
            ?? throw new InvalidOperationException("Diagnostic session not found");

        if (session.CompletedAt != null)
            throw new InvalidOperationException("Diagnostic already completed");

        // Find the placeholder answer for this question
        var answer = await _context.UserAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Question)
                .ThenInclude(q => q.Topic)
            .FirstOrDefaultAsync(a =>
                a.TestSessionId == dto.SessionId &&
                a.QuestionId == dto.QuestionId &&
                a.TimeSpentSeconds == -1)
            ?? throw new InvalidOperationException("Question not found in this diagnostic or already answered");

        var question = answer.Question;
        var selectedOption = question.AnswerOptions.FirstOrDefault(o => o.Id == dto.AnswerOptionId)
            ?? throw new InvalidOperationException("Invalid answer option");
        var correctOption = question.AnswerOptions.First(o => o.IsCorrect);
        var isCorrect = selectedOption.IsCorrect;

        // Update the placeholder answer with real data
        answer.AnswerOptionId = dto.AnswerOptionId;
        answer.AnsweredAt = DateTime.UtcNow;
        answer.TimeSpentSeconds = dto.TimeSpentSeconds;

        // Update skill profiles via adaptive engine
        await _adaptiveEngine.UpdateSkillLevelAsync(userId, question.Topic.SkillId, isCorrect);
        await _unitOfWork.SaveChangesAsync();

        // Check if diagnostic is complete
        var allAnswers = await _context.UserAnswers
            .Where(a => a.TestSessionId == dto.SessionId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        var currentIndex = allAnswers.FindIndex(a => a.QuestionId == dto.QuestionId);
        var isCompleted = allAnswers.All(a => a.TimeSpentSeconds != -1);

        if (isCompleted)
        {
            // Complete the session
            var answersWithOptions = await _context.UserAnswers
                .Include(a => a.AnswerOption)
                .Where(a => a.TestSessionId == dto.SessionId)
                .ToListAsync();

            session.CompletedAt = DateTime.UtcNow;
            session.TotalQuestions = answersWithOptions.Count;
            session.CorrectCount = answersWithOptions.Count(a => a.AnswerOption.IsCorrect);
            session.Score = session.TotalQuestions > 0
                ? Math.Round((double)session.CorrectCount / session.TotalQuestions * 100, 1)
                : 0;
            await _unitOfWork.SaveChangesAsync();
        }

        return new DiagnosticAnswerResultDto(
            isCorrect,
            correctOption.Id,
            correctOption.Text,
            question.Explanation,
            currentIndex + 1,
            allAnswers.Count,
            isCompleted
        );
    }

    public async Task<DiagnosticResultDto> GetResultsAsync(int userId, int sessionId)
    {
        var session = await _context.TestSessions
            .Include(s => s.ExamType)
                .ThenInclude(e => e.Sections)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.Mode == DiagnosticMode)
            ?? throw new InvalidOperationException("Diagnostic session not found");

        if (session.CompletedAt == null)
            throw new InvalidOperationException("Diagnostic not yet completed");

        var answers = await _context.UserAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.Topic)
                    .ThenInclude(t => t.Section)
            .Include(a => a.Question)
                .ThenInclude(q => q.AnswerOptions)
            .Include(a => a.AnswerOption)
            .Where(a => a.TestSessionId == sessionId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        var totalQuestions = answers.Count;
        var correctCount = answers.Count(a => a.AnswerOption.IsCorrect);
        var scorePercent = totalQuestions > 0
            ? Math.Round((double)correctCount / totalQuestions * 100, 1) : 0;

        // Get user's theta for score prediction
        var skillProfiles = await _context.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToListAsync();
        var avgTheta = skillProfiles.Count > 0 ? skillProfiles.Average(p => p.Theta) : 0.0;
        var avgSE = skillProfiles.Count > 0 ? skillProfiles.Average(p => p.ThetaSE) : 1.0;

        // Predict score based on exam type
        var sections = session.ExamType.Sections.ToList();
        var maxScore = sections.Sum(s => s.MaxScore);
        var minScore = sections.Sum(s => s.MinScore);
        var scoreRange = maxScore - minScore;

        // Map theta to predicted score (logistic mapping)
        var predictedPercent = 1.0 / (1.0 + Math.Exp(-1.0 * avgTheta)); // 0..1
        var predictedScore = (int)Math.Round(minScore + predictedPercent * scoreRange);
        var predictedMin = (int)Math.Round(minScore + (1.0 / (1.0 + Math.Exp(-1.0 * (avgTheta - 1.645 * avgSE)))) * scoreRange);
        var predictedMax = (int)Math.Round(minScore + (1.0 / (1.0 + Math.Exp(-1.0 * (avgTheta + 1.645 * avgSE)))) * scoreRange);

        // Determine level
        var levelPercent = (double)(predictedScore - minScore) / scoreRange;
        var level = levelPercent switch
        {
            < 0.3 => "Начинающий",
            < 0.5 => "Средний",
            < 0.7 => "Хороший",
            < 0.85 => "Продвинутый",
            _ => "Отличный"
        };

        // Section breakdown
        var sectionResults = answers
            .GroupBy(a => a.Question.Topic.Section?.Name ?? "Other")
            .Select(g => new DiagnosticSectionResultDto(
                g.Key,
                g.Count(),
                g.Count(a => a.AnswerOption.IsCorrect),
                g.Count() > 0
                    ? Math.Round((double)g.Count(a => a.AnswerOption.IsCorrect) / g.Count() * 100, 1)
                    : 0
            ))
            .ToList();

        // Answer review
        var answerReviews = answers.Select(a =>
        {
            var selectedOpt = a.Question.AnswerOptions.First(o => o.Id == a.AnswerOptionId);
            var correctOpt = a.Question.AnswerOptions.First(o => o.IsCorrect);
            return new DiagnosticAnswerReviewDto(
                a.QuestionId,
                a.Question.Text,
                a.Question.Topic.Name,
                a.Question.Difficulty.ToString(),
                selectedOpt.Id,
                selectedOpt.Text,
                correctOpt.Id,
                correctOpt.Text,
                a.AnswerOption.IsCorrect,
                a.Question.Explanation
            );
        }).ToList();

        return new DiagnosticResultDto(
            sessionId,
            session.ExamTypeCode,
            session.ExamType.Name,
            totalQuestions,
            correctCount,
            scorePercent,
            predictedScore,
            predictedMin,
            predictedMax,
            maxScore,
            level,
            sectionResults,
            answerReviews
        );
    }

    // ─── Question Selection ──────────────────────────────────────

    private async Task<List<Question>> SelectDiagnosticQuestionsAsync(string examTypeCode, int userId)
    {
        // Get all questions for this exam type, grouped by section
        var questions = await _context.Questions
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section)
            .Include(q => q.AnswerOptions)
            .Where(q => q.Topic.Section != null && q.Topic.Section.ExamTypeCode == examTypeCode)
            .ToListAsync();

        if (questions.Count == 0)
            throw new InvalidOperationException("No questions available for this exam type");

        // Get already answered question IDs to prefer fresh questions
        var answeredIds = await _context.UserAnswers
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.QuestionId)
            .Distinct()
            .ToListAsync();
        var answeredSet = answeredIds.ToHashSet();

        // Group by section
        var bySection = questions.GroupBy(q => q.Topic.Section!.Name).ToList();
        var questionsPerSection = Math.Max(1, DiagnosticQuestionCount / bySection.Count);
        var remainder = DiagnosticQuestionCount - questionsPerSection * bySection.Count;

        var selected = new List<Question>();
        var rng = Random.Shared;

        foreach (var sectionGroup in bySection)
        {
            var sectionQuestions = sectionGroup.ToList();
            var count = questionsPerSection + (remainder > 0 ? 1 : 0);
            if (remainder > 0) remainder--;

            // Select balanced difficulty: Easy, Medium, Hard
            var byDifficulty = sectionQuestions.GroupBy(q => q.Difficulty).ToDictionary(g => g.Key, g => g.ToList());

            var sectionSelected = new List<Question>();
            var difficulties = new[] { QuestionDifficulty.Easy, QuestionDifficulty.Medium, QuestionDifficulty.Hard };

            foreach (var diff in difficulties)
            {
                if (sectionSelected.Count >= count) break;
                if (!byDifficulty.TryGetValue(diff, out var pool)) continue;

                // Prefer unanswered
                var unanswered = pool.Where(q => !answeredSet.Contains(q.Id)).ToList();
                var source = unanswered.Count > 0 ? unanswered : pool;

                var pick = source.OrderBy(_ => rng.Next()).FirstOrDefault();
                if (pick != null)
                {
                    sectionSelected.Add(pick);
                    // Remove from pool to avoid duplicates
                    pool.Remove(pick);
                }
            }

            // Fill remaining from any difficulty
            while (sectionSelected.Count < count && sectionQuestions.Count > 0)
            {
                var remaining = sectionQuestions.Where(q => !sectionSelected.Contains(q)).ToList();
                if (remaining.Count == 0) break;
                var unanswered = remaining.Where(q => !answeredSet.Contains(q.Id)).ToList();
                var source = unanswered.Count > 0 ? unanswered : remaining;
                sectionSelected.Add(source[rng.Next(source.Count)]);
            }

            selected.AddRange(sectionSelected.Take(count));
        }

        // If we still need more, fill from remaining
        while (selected.Count < DiagnosticQuestionCount && questions.Count > selected.Count)
        {
            var remaining = questions.Where(q => !selected.Contains(q)).ToList();
            if (remaining.Count == 0) break;
            selected.Add(remaining[rng.Next(remaining.Count)]);
        }

        // Shuffle the final selection
        return selected.OrderBy(_ => rng.Next()).ToList();
    }
}
