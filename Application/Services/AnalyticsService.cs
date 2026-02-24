using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly UniStartDbContext _context;

    public AnalyticsService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<SkillAnalyticsDto> GetUserAnalyticsAsync(int userId)
    {
        var userAnswers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var totalQuestionsAnswered = userAnswers.Count;
        var correctAnswers = userAnswers.Count(ua => ua.AnswerOption.IsCorrect);
        var overallAccuracy = totalQuestionsAnswered > 0 
            ? (double)correctAnswers / totalQuestionsAnswered * 100 
            : 0;

        var skillProfiles = await GetUserSkillsAsync(userId);
        var recentProgress = await GetRecentProgressAsync(userId);

        return new SkillAnalyticsDto(
            totalQuestionsAnswered,
            correctAnswers,
            Math.Round(overallAccuracy, 2),
            skillProfiles,
            recentProgress
        );
    }

    public async Task<IEnumerable<UserSkillProfileDto>> GetUserSkillsAsync(int userId)
    {
        var profiles = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Level)
            .ToListAsync();

        return profiles.Select(p => new UserSkillProfileDto(
            p.SkillId,
            p.Skill.Name,
            p.Skill.Code,
            p.Level,
            p.LastUpdated
        ));
    }

    // ─── Stage 4: Enhanced Analytics ───────────────────────────────

    public async Task<DashboardDto> GetDashboardAsync(int userId)
    {
        var userAnswers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Include(ua => ua.Question)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var totalQuestionsAnswered = userAnswers.Count;
        var correctAnswers = userAnswers.Count(ua => ua.AnswerOption.IsCorrect);
        var overallAccuracy = totalQuestionsAnswered > 0 
            ? Math.Round((double)correctAnswers / totalQuestionsAnswered * 100, 2) 
            : 0;

        var skillProfiles = await GetUserSkillsAsync(userId);
        var skillHistory = await GetSkillHistoryAsync(userId, 30);
        var activityHeatmap = await GetActivityHeatmapAsync(userId, 90);
        var difficultyBreakdown = await GetDifficultyBreakdownAsync(userId);
        var (currentStreak, bestStreak) = await CalculateStreaksAsync(userId);

        return new DashboardDto(
            totalQuestionsAnswered,
            correctAnswers,
            overallAccuracy,
            currentStreak,
            bestStreak,
            skillProfiles,
            skillHistory,
            activityHeatmap,
            difficultyBreakdown
        );
    }

    public async Task<IEnumerable<SkillHistoryPointDto>> GetSkillHistoryAsync(int userId, int days = 30)
    {
        // Build skill level history from user answers chronologically
        var cutoff = DateTime.UtcNow.AddDays(-days);
        
        var answers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Include(ua => ua.Question)
                .ThenInclude(q => q.Topic)
                    .ThenInclude(t => t.Skill)
            .Where(ua => ua.UserId == userId && ua.AnsweredAt >= cutoff)
            .OrderBy(ua => ua.AnsweredAt)
            .ToListAsync();

        if (!answers.Any())
            return Enumerable.Empty<SkillHistoryPointDto>();

        // Reconstruct skill levels over time by grouping per day per skill
        var skillLevels = new Dictionary<int, int>(); // skillId → current level
        var currentProfiles = await _context.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToListAsync();
        
        // Start from current levels and work backwards isn't practical,
        // so we approximate from the initial level (50) and replay
        foreach (var p in currentProfiles)
            skillLevels[p.SkillId] = 50; // Start at default

        var history = new List<SkillHistoryPointDto>();
        
        // Group by date and replay
        var dailyGroups = answers.GroupBy(a => a.AnsweredAt.Date).OrderBy(g => g.Key);
        
        foreach (var dayGroup in dailyGroups)
        {
            foreach (var answer in dayGroup.OrderBy(a => a.AnsweredAt))
            {
                var skillId = answer.Question.Topic.SkillId;
                if (!skillLevels.ContainsKey(skillId))
                    skillLevels[skillId] = 50;

                var change = answer.AnswerOption.IsCorrect ? 5 : -3;
                skillLevels[skillId] = Math.Clamp(skillLevels[skillId] + change, 0, 100);
            }

            // Emit one point per skill per day
            foreach (var (skillId, level) in skillLevels)
            {
                var skill = answers.FirstOrDefault(a => a.Question.Topic.SkillId == skillId)?.Question.Topic.Skill;
                if (skill != null)
                {
                    history.Add(new SkillHistoryPointDto(
                        skill.Name,
                        skill.Code,
                        level,
                        dayGroup.Key
                    ));
                }
            }
        }

        return history;
    }

    public async Task<IEnumerable<DailyActivityDto>> GetActivityHeatmapAsync(int userId, int days = 90)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var answers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Where(ua => ua.UserId == userId && ua.AnsweredAt >= cutoff)
            .ToListAsync();

        var dailyActivity = answers
            .GroupBy(ua => ua.AnsweredAt.Date)
            .Select(g => new DailyActivityDto(
                g.Key,
                g.Count(),
                g.Count(ua => ua.AnswerOption.IsCorrect)
            ))
            .OrderBy(d => d.Date)
            .ToList();

        return dailyActivity;
    }

    public async Task<IEnumerable<DifficultyStatsDto>> GetDifficultyBreakdownAsync(int userId)
    {
        var answers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Include(ua => ua.Question)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var stats = answers
            .GroupBy(ua => ua.Question.Difficulty)
            .Select(g => new DifficultyStatsDto(
                g.Key.ToString(),
                g.Count(),
                g.Count(ua => ua.AnswerOption.IsCorrect),
                g.Count() > 0 
                    ? Math.Round((double)g.Count(ua => ua.AnswerOption.IsCorrect) / g.Count() * 100, 1) 
                    : 0
            ))
            .ToList();

        return stats;
    }

    // ─── Test Sessions ─────────────────────────────────────────────

    public async Task<TestSessionSummaryDto> StartSessionAsync(int userId, string examTypeCode, string mode)
    {
        var examType = await _context.ExamTypes.FindAsync(examTypeCode)
            ?? throw new ArgumentException($"Exam type '{examTypeCode}' not found");

        var session = new TestSession
        {
            UserId = userId,
            ExamTypeCode = examTypeCode,
            Mode = mode,
            StartedAt = DateTime.UtcNow,
            TotalQuestions = 0,
            CorrectCount = 0
        };

        _context.TestSessions.Add(session);
        await _context.SaveChangesAsync();

        return new TestSessionSummaryDto(
            session.Id, session.ExamTypeCode, examType.Name, session.Mode,
            session.StartedAt, session.CompletedAt,
            session.TotalQuestions, session.CorrectCount, session.Score
        );
    }

    public async Task<TestSessionSummaryDto> CompleteSessionAsync(int sessionId)
    {
        var session = await _context.TestSessions
            .Include(s => s.ExamType)
            .Include(s => s.Answers)
                .ThenInclude(a => a.AnswerOption)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new ArgumentException("Session not found");

        session.CompletedAt = DateTime.UtcNow;
        session.TotalQuestions = session.Answers.Count;
        session.CorrectCount = session.Answers.Count(a => a.AnswerOption.IsCorrect);
        session.Score = session.TotalQuestions > 0 
            ? Math.Round((double)session.CorrectCount / session.TotalQuestions * 100, 1) 
            : 0;

        await _context.SaveChangesAsync();

        return new TestSessionSummaryDto(
            session.Id, session.ExamTypeCode, session.ExamType.Name, session.Mode,
            session.StartedAt, session.CompletedAt,
            session.TotalQuestions, session.CorrectCount, session.Score
        );
    }

    public async Task<IEnumerable<TestSessionSummaryDto>> GetSessionsAsync(int userId, int page = 1, int pageSize = 10)
    {
        var sessions = await _context.TestSessions
            .Include(s => s.ExamType)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new TestSessionSummaryDto(
                s.Id, s.ExamTypeCode, s.ExamType.Name, s.Mode,
                s.StartedAt, s.CompletedAt,
                s.TotalQuestions, s.CorrectCount, s.Score
            ))
            .ToListAsync();

        return sessions;
    }

    public async Task<TestSessionDetailDto?> GetSessionDetailAsync(int userId, int sessionId)
    {
        var session = await _context.TestSessions
            .Include(s => s.ExamType)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q.Topic)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q.AnswerOptions)
            .Include(s => s.Answers)
                .ThenInclude(a => a.AnswerOption)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null) return null;

        var answers = session.Answers.OrderBy(a => a.AnsweredAt).Select(a =>
        {
            var correctOption = a.Question.AnswerOptions.First(o => o.IsCorrect);
            return new SessionAnswerDto(
                a.QuestionId,
                a.Question.Text,
                a.Question.Topic.Name,
                a.Question.Difficulty.ToString(),
                a.AnswerOptionId,
                a.AnswerOption.Text,
                correctOption.Id,
                correctOption.Text,
                a.AnswerOption.IsCorrect,
                a.Question.Explanation,
                a.TimeSpentSeconds
            );
        });

        return new TestSessionDetailDto(
            session.Id, session.ExamTypeCode, session.ExamType.Name, session.Mode,
            session.StartedAt, session.CompletedAt,
            session.TotalQuestions, session.CorrectCount, session.Score,
            answers
        );
    }

    // ─── Private helpers ───────────────────────────────────────────

    private async Task<IEnumerable<SkillProgressDto>> GetRecentProgressAsync(int userId)
    {
        var recentUpdates = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.LastUpdated)
            .Take(10)
            .Select(p => new SkillProgressDto(
                p.Skill.Name,
                p.Level - 5,
                p.Level,
                p.LastUpdated
            ))
            .ToListAsync();

        return recentUpdates;
    }

    private async Task<(int currentStreak, int bestStreak)> CalculateStreaksAsync(int userId)
    {
        var dates = await _context.UserAnswers
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AnsweredAt)
            .ToListAsync();

        var activeDates = dates
            .Select(d => d.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (!activeDates.Any())
            return (0, 0);

        var today = DateTime.UtcNow.Date;
        var currentStreak = 0;
        var bestStreak = 0;
        var streak = 0;
        
        // Check if today or yesterday is in the list to start current streak
        var startDate = activeDates.First();
        var isCurrentlyActive = (today - startDate).Days <= 1;

        for (var i = 0; i < activeDates.Count; i++)
        {
            if (i == 0)
            {
                streak = 1;
            }
            else
            {
                var diff = (activeDates[i - 1] - activeDates[i]).Days;
                if (diff == 1)
                    streak++;
                else
                {
                    if (i == 1 || isCurrentlyActive)
                        currentStreak = streak;
                    bestStreak = Math.Max(bestStreak, streak);
                    streak = 1;
                    isCurrentlyActive = false;
                }
            }
        }

        if (isCurrentlyActive && currentStreak == 0)
            currentStreak = streak;
        bestStreak = Math.Max(bestStreak, streak);

        return (currentStreak, bestStreak);
    }
}
