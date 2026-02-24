using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class RecommendationService : IRecommendationService
{
    private readonly UniStartDbContext _db;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(UniStartDbContext db, ILogger<RecommendationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════
    //  DAILY BRIEFING
    // ═══════════════════════════════════════════════════════

    public async Task<DailyBriefingDto> GetDailyBriefingAsync(int userId)
    {
        var streak = await GetStreakAsync(userId);
        var recommendations = await GenerateDailyRecommendationsAsync(userId);
        var newMilestones = await CheckAndAwardMilestonesAsync(userId);
        var recentMilestones = await GetRecentMilestonesAsync(userId, 7);
        var yesterday = await GetDaySummaryAsync(userId, DateTime.UtcNow.Date.AddDays(-1));

        return new DailyBriefingDto(
            CurrentStreak: streak.CurrentStreak,
            LongestStreak: streak.LongestStreak,
            Recommendations: recommendations,
            RecentMilestones: recentMilestones.ToList(),
            Streak: streak,
            YesterdaySummary: yesterday
        );
    }

    // ═══════════════════════════════════════════════════════
    //  AFTER-SESSION RECOMMENDATIONS
    // ═══════════════════════════════════════════════════════

    public async Task<AfterSessionDto> GetAfterSessionRecommendationsAsync(int userId, int sessionId)
    {
        var session = await _db.TestSessions
            .Include(s => s.Answers)
                .ThenInclude(a => a.AnswerOption)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question!)
                .ThenInclude(q => q.Topic)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
            return new AfterSessionDto(new List<RecommendationDto>());

        var recs = new List<RecommendationDto>();

        // ─── Analyze errors per topic ────────────────────
        var topicErrors = session.Answers
            .Where(a => !a.AnswerOption!.IsCorrect)
            .GroupBy(a => a.Question!.Topic)
            .Select(g => new { Topic = g.Key!, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        foreach (var te in topicErrors.Take(3))
        {
            recs.Add(new RecommendationDto(
                Type: "after_session",
                Priority: te.Count >= 3 ? "high" : "medium",
                Title: $"Повторите {te.Topic.Name}",
                Description: $"В этой сессии {te.Count} ошибок по теме «{te.Topic.Name}». Рекомендуем дополнительную практику.",
                Icon: "📝",
                ActionLabel: "Практика",
                ActionUrl: $"/test?topicId={te.Topic.Id}",
                Metadata: new Dictionary<string, object>
                {
                    ["topicId"] = te.Topic.Id,
                    ["errorCount"] = te.Count
                }
            ));
        }

        // ─── Praise strong topics ────────────────────────
        var topicSuccess = session.Answers
            .Where(a => a.AnswerOption!.IsCorrect)
            .GroupBy(a => a.Question!.Topic)
            .Select(g => new { Topic = g.Key!, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var perfect = topicSuccess
            .Where(ts => !topicErrors.Any(te => te.Topic.Id == ts.Topic.Id))
            .Take(1);

        foreach (var ts in perfect)
        {
            recs.Add(new RecommendationDto(
                Type: "after_session",
                Priority: "low",
                Title: $"Отлично по {ts.Topic.Name}!",
                Description: $"Все {ts.Count} ответов правильные. Попробуйте более сложные вопросы!",
                Icon: "🌟",
                ActionLabel: "Hard-режим",
                ActionUrl: $"/test?topicId={ts.Topic.Id}&difficulty=Hard",
                Metadata: new Dictionary<string, object>
                {
                    ["topicId"] = ts.Topic.Id,
                    ["correctCount"] = ts.Count
                }
            ));
        }

        // ─── Accuracy-based advice ───────────────────────
        if (session.TotalQuestions > 0)
        {
            var accuracy = (double)session.CorrectCount / session.TotalQuestions * 100;
            if (accuracy < 50)
            {
                recs.Add(new RecommendationDto(
                    Type: "after_session",
                    Priority: "high",
                    Title: "Не сдавайтесь!",
                    Description: $"Точность {accuracy:F0}% — попробуйте вернуться к основам. Начните с Practice-режима.",
                    Icon: "💪",
                    ActionLabel: "Practice Mode",
                    ActionUrl: "/test?mode=practice",
                    Metadata: null
                ));
            }
            else if (accuracy >= 80)
            {
                recs.Add(new RecommendationDto(
                    Type: "after_session",
                    Priority: "low",
                    Title: "Отличный результат!",
                    Description: $"Точность {accuracy:F0}%! Попробуйте Exam Mode для подготовки к реальному экзамену.",
                    Icon: "🎯",
                    ActionLabel: "Exam Mode",
                    ActionUrl: "/test?mode=exam",
                    Metadata: null
                ));
            }
        }

        // ─── Check for new milestones ────────────────────
        var newMilestones = await CheckAndAwardMilestonesAsync(userId);
        foreach (var m in newMilestones)
        {
            recs.Add(new RecommendationDto(
                Type: "milestone",
                Priority: "medium",
                Title: $"{m.Icon} {m.Title}",
                Description: m.Description,
                Icon: m.Icon,
                ActionLabel: null,
                ActionUrl: null,
                Metadata: new Dictionary<string, object> { ["milestoneCode"] = m.Code }
            ));
        }

        return new AfterSessionDto(recs);
    }

    // ═══════════════════════════════════════════════════════
    //  STREAK
    // ═══════════════════════════════════════════════════════

    public async Task<StreakDto> GetStreakAsync(int userId)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        // Get all unique study days (dates when user answered questions)
        var studyDays = await _db.UserAnswers
            .Where(a => a.UserId == userId)
            .Select(a => a.AnsweredAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        if (!studyDays.Any())
        {
            return new StreakDto(
                CurrentStreak: 0, LongestStreak: 0,
                StudiedToday: false, LastStudyDate: null, TotalStudyDays: 0
            );
        }

        var studiedToday = studyDays.Contains(today);
        var lastStudyDate = studyDays.First();

        // Calculate current streak
        int currentStreak = 0;
        var checkDate = studiedToday ? today : today.AddDays(-1);
        foreach (var day in studyDays)
        {
            if (day == checkDate)
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
            else if (day < checkDate)
            {
                break;
            }
        }

        // If user didn't study today and didn't study yesterday, streak is 0
        if (!studiedToday && lastStudyDate < today.AddDays(-1))
            currentStreak = 0;

        // Calculate longest streak ever
        int longestStreak = 0;
        int tempStreak = 1;
        var sortedDays = studyDays.OrderBy(d => d).ToList();
        for (int i = 1; i < sortedDays.Count; i++)
        {
            if ((sortedDays[i] - sortedDays[i - 1]).TotalDays == 1)
            {
                tempStreak++;
            }
            else
            {
                longestStreak = Math.Max(longestStreak, tempStreak);
                tempStreak = 1;
            }
        }
        longestStreak = Math.Max(longestStreak, tempStreak);

        return new StreakDto(
            CurrentStreak: currentStreak,
            LongestStreak: longestStreak,
            StudiedToday: studiedToday,
            LastStudyDate: DateTime.SpecifyKind(lastStudyDate, DateTimeKind.Utc),
            TotalStudyDays: studyDays.Count
        );
    }

    // ═══════════════════════════════════════════════════════
    //  MILESTONES
    // ═══════════════════════════════════════════════════════

    public async Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int userId)
    {
        var milestones = await _db.UserMilestones
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.AchievedAt)
            .ToListAsync();

        return milestones.Select(m => new MilestoneDto(
            Id: m.Id, Code: m.Code, Title: m.Title,
            Description: m.Description, Icon: m.Icon,
            AchievedAt: m.AchievedAt, IsNew: false
        ));
    }

    public async Task<IEnumerable<MilestoneDto>> CheckAndAwardMilestonesAsync(int userId)
    {
        var existingList = await _db.UserMilestones
            .Where(m => m.UserId == userId)
            .Select(m => m.Code)
            .ToListAsync();
        var existing = existingList.ToHashSet();

        var newMilestones = new List<UserMilestone>();

        // ─── Questions answered milestones ───────────────
        var totalAnswers = await _db.UserAnswers.CountAsync(a => a.UserId == userId);
        var answerMilestones = new (int count, string code, string title, string icon)[]
        {
            (10, "Q10", "Первые шаги", "🐣"),
            (50, "Q50", "Полсотни!", "📚"),
            (100, "Q100", "Сотня вопросов", "💯"),
            (250, "Q250", "Настоящий студент", "🎓"),
            (500, "Q500", "Полтысячи!", "🏅"),
            (1000, "Q1000", "Тысячник", "🏆"),
        };

        foreach (var (count, code, title, icon) in answerMilestones)
        {
            if (totalAnswers >= count && !existing.Contains(code))
            {
                newMilestones.Add(new UserMilestone
                {
                    UserId = userId, Code = code, Title = title,
                    Description = $"Вы ответили на {count} вопросов!",
                    Icon = icon, AchievedAt = DateTime.UtcNow
                });
            }
        }

        // ─── Streak milestones ───────────────────────────
        var streak = await GetStreakAsync(userId);
        var streakMilestones = new (int days, string code, string title, string icon)[]
        {
            (3, "STREAK_3", "3 дня подряд!", "🔥"),
            (7, "STREAK_7", "Неделя без пропусков", "⚡"),
            (14, "STREAK_14", "Две недели!", "💎"),
            (30, "STREAK_30", "Месяц подряд!", "👑"),
        };

        foreach (var (days, code, title, icon) in streakMilestones)
        {
            if (streak.CurrentStreak >= days && !existing.Contains(code))
            {
                newMilestones.Add(new UserMilestone
                {
                    UserId = userId, Code = code, Title = title,
                    Description = $"Вы занимались {days} дней подряд!",
                    Icon = icon, AchievedAt = DateTime.UtcNow
                });
            }
        }

        // ─── Mastery milestones (per skill) ──────────────
        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .Include(p => p.Skill)
            .ToListAsync();

        foreach (var profile in profiles)
        {
            var masteryCode = $"MASTERY_{profile.Skill.Code}";
            if (profile.Level >= 80 && !existing.Contains(masteryCode))
            {
                newMilestones.Add(new UserMilestone
                {
                    UserId = userId, Code = masteryCode,
                    Title = $"Мастер: {profile.Skill.Name}",
                    Description = $"Уровень навыка «{profile.Skill.Name}» достиг 80%!",
                    Icon = "⭐", AchievedAt = DateTime.UtcNow
                });
            }
        }

        // ─── Accuracy milestones ─────────────────────────
        if (totalAnswers >= 20)
        {
            var totalCorrect = await _db.UserAnswers
                .Where(a => a.UserId == userId && a.AnswerOption!.IsCorrect)
                .CountAsync();
            var overallAccuracy = (double)totalCorrect / totalAnswers * 100;

            if (overallAccuracy >= 90 && !existing.Contains("ACC_90"))
            {
                newMilestones.Add(new UserMilestone
                {
                    UserId = userId, Code = "ACC_90",
                    Title = "Снайпер!",
                    Description = "Общая точность ≥ 90% (минимум 20 ответов)",
                    Icon = "🎯", AchievedAt = DateTime.UtcNow
                });
            }
        }

        // ─── Session count milestones ────────────────────
        var sessionCount = await _db.TestSessions.CountAsync(s => s.UserId == userId);
        if (sessionCount >= 10 && !existing.Contains("SESSIONS_10"))
        {
            newMilestones.Add(new UserMilestone
            {
                UserId = userId, Code = "SESSIONS_10",
                Title = "10 тестов позади!",
                Description = "Вы завершили 10 тестовых сессий",
                Icon = "📋", AchievedAt = DateTime.UtcNow
            });
        }

        // Persist new milestones
        if (newMilestones.Any())
        {
            _db.UserMilestones.AddRange(newMilestones);
            await _db.SaveChangesAsync();
        }

        return newMilestones.Select(m => new MilestoneDto(
            Id: m.Id, Code: m.Code, Title: m.Title,
            Description: m.Description, Icon: m.Icon,
            AchievedAt: m.AchievedAt, IsNew: true
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  DAILY RECOMMENDATIONS (PRIVATE)
    // ═══════════════════════════════════════════════════════

    private async Task<List<RecommendationDto>> GenerateDailyRecommendationsAsync(int userId)
    {
        var recs = new List<RecommendationDto>();
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        // ─── Forgetting Curve: topics needing review ─────
        var topicLastAnswer = await _db.UserAnswers
            .Where(a => a.UserId == userId)
            .GroupBy(a => a.Question!.Topic)
            .Select(g => new
            {
                Topic = g.Key!,
                LastAnswered = g.Max(a => a.AnsweredAt),
                TotalCorrect = g.Count(a => a.AnswerOption!.IsCorrect),
                Total = g.Count()
            })
            .OrderBy(x => x.LastAnswered)
            .ToListAsync();

        foreach (var ta in topicLastAnswer)
        {
            var daysSince = (DateTime.UtcNow - ta.LastAnswered).TotalDays;
            var stability = IrtMath.CalculateStability(ta.TotalCorrect);
            var retention = IrtMath.RetentionProbability(daysSince, stability);

            if (retention < 0.7 && daysSince >= 2) // Retention dropping below 70%
            {
                recs.Add(new RecommendationDto(
                    Type: "daily",
                    Priority: retention < 0.4 ? "high" : "medium",
                    Title: $"Повторите {ta.Topic.Name}",
                    Description: $"Последнее занятие {daysSince:F0} дней назад. Оценка запоминания: {retention * 100:F0}%.",
                    Icon: "🧠",
                    ActionLabel: "Повторить",
                    ActionUrl: $"/test?topicId={ta.Topic.Id}",
                    Metadata: new Dictionary<string, object>
                    {
                        ["topicId"] = ta.Topic.Id,
                        ["daysSince"] = Math.Round(daysSince, 1),
                        ["retention"] = Math.Round(retention * 100, 1)
                    }
                ));
            }
        }

        // ─── Weak topics needing practice ────────────────
        var weakProfiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId && p.Level < 40)
            .Include(p => p.Skill)
            .OrderBy(p => p.Level)
            .Take(3)
            .ToListAsync();

        foreach (var wp in weakProfiles)
        {
            // Find a topic for this skill
            var topic = await _db.Topics
                .FirstOrDefaultAsync(t => t.SkillId == wp.SkillId);

            if (topic != null)
            {
                recs.Add(new RecommendationDto(
                    Type: "daily",
                    Priority: "high",
                    Title: $"Подтяните {wp.Skill.Name}",
                    Description: $"Уровень {wp.Level}% — рекомендуем практику по «{topic.Name}»",
                    Icon: "⚠️",
                    ActionLabel: "Практика",
                    ActionUrl: $"/test?topicId={topic.Id}",
                    Metadata: new Dictionary<string, object>
                    {
                        ["skillId"] = wp.SkillId,
                        ["level"] = wp.Level
                    }
                ));
            }
        }

        // ─── Mode recommendation based on goal ──────────
        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive);

        if (goal != null)
        {
            var daysUntilExam = (goal.TargetDate - today).TotalDays;
            if (daysUntilExam <= 14 && daysUntilExam > 0)
            {
                recs.Add(new RecommendationDto(
                    Type: "mode",
                    Priority: "high",
                    Title: "Экзамен через " + (int)daysUntilExam + " дней!",
                    Description: "Попробуйте Exam Mode для тренировки в условиях таймера",
                    Icon: "⏱️",
                    ActionLabel: "Exam Mode",
                    ActionUrl: "/test?mode=exam",
                    Metadata: new Dictionary<string, object>
                    {
                        ["daysUntilExam"] = (int)daysUntilExam,
                        ["examTypeCode"] = goal.ExamTypeCode
                    }
                ));
            }
            else if (daysUntilExam > 14)
            {
                // Balanced recommendation
                var recentExamSessions = await _db.TestSessions
                    .Where(s => s.UserId == userId && s.Mode == "exam")
                    .CountAsync();

                if (recentExamSessions == 0)
                {
                    recs.Add(new RecommendationDto(
                        Type: "mode",
                        Priority: "low",
                        Title: "Попробуйте Exam Mode",
                        Description: "Вы ещё не пробовали экзаменационный режим. Потренируйтесь с таймером!",
                        Icon: "📝",
                        ActionLabel: "Попробовать",
                        ActionUrl: "/test?mode=exam",
                        Metadata: null
                    ));
                }
            }
        }

        // ─── Streak motivation ───────────────────────────
        var streak = await GetStreakAsync(userId);
        if (streak.CurrentStreak > 0 && !streak.StudiedToday)
        {
            recs.Add(new RecommendationDto(
                Type: "streak",
                Priority: "medium",
                Title: $"Серия {streak.CurrentStreak} дней!",
                Description: "Не прерывайте серию — ответьте хотя бы на 5 вопросов сегодня",
                Icon: "🔥",
                ActionLabel: "Начать",
                ActionUrl: "/test",
                Metadata: new Dictionary<string, object>
                {
                    ["currentStreak"] = streak.CurrentStreak
                }
            ));
        }

        // Sort by priority
        var priorityOrder = new Dictionary<string, int> { ["high"] = 0, ["medium"] = 1, ["low"] = 2 };
        return recs
            .OrderBy(r => priorityOrder.GetValueOrDefault(r.Priority, 99))
            .Take(8) // Max 8 recommendations
            .ToList();
    }

    // ═══════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════

    private async Task<IEnumerable<MilestoneDto>> GetRecentMilestonesAsync(int userId, int days)
    {
        var since = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-days), DateTimeKind.Utc);
        var milestones = await _db.UserMilestones
            .Where(m => m.UserId == userId && m.AchievedAt >= since)
            .OrderByDescending(m => m.AchievedAt)
            .ToListAsync();

        return milestones.Select(m => new MilestoneDto(
            Id: m.Id, Code: m.Code, Title: m.Title,
            Description: m.Description, Icon: m.Icon,
            AchievedAt: m.AchievedAt,
            IsNew: (DateTime.UtcNow - m.AchievedAt).TotalHours < 24
        ));
    }

    private async Task<DailySummaryDto?> GetDaySummaryAsync(int userId, DateTime date)
    {
        var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var answers = await _db.UserAnswers
            .Where(a => a.UserId == userId && a.AnsweredAt >= dayStart && a.AnsweredAt < dayEnd)
            .Include(a => a.AnswerOption)
            .Include(a => a.Question)
            .ToListAsync();

        if (!answers.Any()) return null;

        var correct = answers.Count(a => a.AnswerOption!.IsCorrect);
        var totalTime = answers.Sum(a => a.TimeSpentSeconds ?? 0);
        var topics = answers.Select(a => a.Question!.TopicId).Distinct().Count();

        return new DailySummaryDto(
            QuestionsAnswered: answers.Count,
            CorrectAnswers: correct,
            Accuracy: Math.Round((double)correct / answers.Count * 100, 1),
            MinutesSpent: totalTime / 60,
            TopicsStudied: topics
        );
    }
}
