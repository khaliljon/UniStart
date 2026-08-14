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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RecommendationService(UniStartDbContext db, ILogger<RecommendationService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetLang()
    {
        var lang = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].FirstOrDefault();
        return lang switch { "kz" => "kz", "en" => "en", _ => "ru" };
    }

    private static string L(string lang, string ru, string kz, string en)
        => lang switch { "kz" => kz, "en" => en, _ => ru };


    public async Task<DailyBriefingDto> GetDailyBriefingAsync(int userId, List<int>? sectionIds = null)
    {
        var streak = await GetStreakAsync(userId);
        var recommendations = await GenerateDailyRecommendationsAsync(userId, sectionIds);
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


    public async Task<AfterSessionDto> GetAfterSessionRecommendationsAsync(int userId, int sessionId)
    {
        var lang = GetLang();
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
                Title: L(lang, $"Повторите {te.Topic.Name}", $"{te.Topic.Name} қайталаңыз", $"Review {te.Topic.Name}"),
                Description: L(lang,
                    $"В этой сессии {te.Count} ошибок по теме «{te.Topic.Name}». Рекомендуем дополнительную практику.",
                    $"Бұл сессияда «{te.Topic.Name}» тақырыбында {te.Count} қате. Қосымша жаттығу ұсынамыз.",
                    $"You made {te.Count} mistakes on \"{te.Topic.Name}\" this session. Extra practice recommended."),
                Icon: null,
                ActionLabel: L(lang, "Практика", "Жаттығу", "Practice"),
                ActionUrl: $"/learn?tab=practice&topicId={te.Topic.Id}",
                Metadata: new Dictionary<string, object>
                {
                    ["topicId"] = te.Topic.Id,
                    ["errorCount"] = te.Count
                }
            ));
        }

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
                Title: L(lang, $"Отлично по {ts.Topic.Name}!", $"{ts.Topic.Name} бойынша тамаша!", $"Great job on {ts.Topic.Name}!"),
                Description: L(lang,
                    $"Все {ts.Count} ответов правильные. Попробуйте более сложные вопросы!",
                    $"Барлық {ts.Count} жауап дұрыс. Күрделірек сұрақтарды қолданып көріңіз!",
                    $"All {ts.Count} answers correct. Try harder questions!"),
                Icon: null,
                ActionLabel: L(lang, "Hard-режим", "Hard режим", "Hard mode"),
                ActionUrl: $"/learn?tab=practice&topicId={ts.Topic.Id}",
                Metadata: new Dictionary<string, object>
                {
                    ["topicId"] = ts.Topic.Id,
                    ["correctCount"] = ts.Count
                }
            ));
        }

        if (session.TotalQuestions > 0)
        {
            var accuracy = (double)session.CorrectCount / session.TotalQuestions * 100;
            if (accuracy < 50)
            {
                recs.Add(new RecommendationDto(
                    Type: "after_session",
                    Priority: "high",
                    Title: L(lang, "Не сдавайтесь!", "Тоқтамаңыз!", "Don't give up!"),
                    Description: L(lang,
                        $"Точность {accuracy:F0}% — попробуйте вернуться к основам. Начните с Practice-режима.",
                        $"Дәлдік {accuracy:F0}% — негіздерге оралып көріңіз. Practice режимінен бастаңыз.",
                        $"Accuracy {accuracy:F0}% — try going back to basics. Start with Practice mode."),
                    Icon: null,
                    ActionLabel: L(lang, "Практика", "Жаттығу", "Practice"),
                    ActionUrl: "/learn",
                    Metadata: null
                ));
            }
            else if (accuracy >= 80)
            {
                recs.Add(new RecommendationDto(
                    Type: "after_session",
                    Priority: "low",
                    Title: L(lang, "Отличный результат!", "Тамаша нәтиже!", "Great result!"),
                    Description: L(lang,
                        $"Точность {accuracy:F0}%! Попробуйте Mock Exam для подготовки к реальному экзамену.",
                        $"Дәлдік {accuracy:F0}%! Нақты емтиханға дайындалу үшін Mock Exam қолданып көріңіз.",
                        $"Accuracy {accuracy:F0}%! Try a Mock Exam to prepare for the real test."),
                    Icon: null,
                    ActionLabel: "Mock Exam",
                    ActionUrl: "/learn?tab=mock",
                    Metadata: null
                ));
            }
        }

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


    public async Task<StreakDto> GetStreakAsync(int userId)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

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

        if (!studiedToday && lastStudyDate < today.AddDays(-1))
            currentStreak = 0;

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

        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .Include(p => p.Section)
            .ToListAsync();

        foreach (var profile in profiles)
        {
            var masteryCode = $"MASTERY_SEC_{profile.SectionId}";
            if (profile.Level >= 80 && !existing.Contains(masteryCode))
            {
                newMilestones.Add(new UserMilestone
                {
                    UserId = userId, Code = masteryCode,
                    Title = $"Мастер: {profile.Section.Name}",
                    Description = $"Уровень «{profile.Section.Name}» достиг 80%!",
                    Icon = "⭐", AchievedAt = DateTime.UtcNow
                });
            }
        }

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


    private async Task<List<RecommendationDto>> GenerateDailyRecommendationsAsync(int userId, List<int>? sectionIds = null)
    {
        var recs = new List<RecommendationDto>();
        var lang = GetLang();
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var filterBySections = sectionIds is { Count: > 0 };
        var sectionIdSet = filterBySections ? new HashSet<int>(sectionIds!) : null;

        var topicQuery = _db.UserAnswers
            .Where(a => a.UserId == userId);
        if (filterBySections)
            topicQuery = topicQuery.Where(a => a.Question!.Topic!.SectionId != null && sectionIdSet!.Contains(a.Question!.Topic!.SectionId!.Value));

        var topicLastAnswer = await topicQuery
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

            if (retention < 0.7 && daysSince >= 2)
            {
                recs.Add(new RecommendationDto(
                    Type: "daily",
                    Priority: retention < 0.4 ? "high" : "medium",
                    Title: L(lang, $"Повторите {ta.Topic.Name}", $"{ta.Topic.Name} қайталаңыз", $"Review {ta.Topic.Name}"),
                    Description: L(lang,
                        $"Последнее занятие {daysSince:F0} дней назад. Оценка запоминания: {retention * 100:F0}%.",
                        $"Соңғы сабақ {daysSince:F0} күн бұрын. Есте сақтау бағасы: {retention * 100:F0}%.",
                        $"Last studied {daysSince:F0} days ago. Retention estimate: {retention * 100:F0}%."),
                    Icon: null,
                    ActionLabel: L(lang, "Повторить", "Қайталау", "Review"),
                    ActionUrl: $"/learn?tab=practice&topicId={ta.Topic.Id}",
                    Metadata: new Dictionary<string, object>
                    {
                        ["topicId"] = ta.Topic.Id,
                        ["daysSince"] = Math.Round(daysSince, 1),
                        ["retention"] = Math.Round(retention * 100, 1)
                    }
                ));
            }
        }

        var weakProfiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId && p.Level < 40)
            .Include(p => p.Section)
            .OrderBy(p => p.Level)
            .Take(3)
            .ToListAsync();

        var masteredTopicIds = new HashSet<int>();
        var userTopicAnswers = await _db.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Include(ua => ua.Question)
            .Where(ua => ua.UserId == userId)
            .GroupBy(ua => ua.Question.TopicId)
            .Select(g => new {
                TopicId = g.Key,
                Questions = g.GroupBy(ua => ua.QuestionId)
                    .Select(qg => qg.OrderByDescending(ua => ua.AnsweredAt).First().AnswerOption!.IsCorrect)
            })
            .ToListAsync();
        
        var topicQuestionTotals = await _db.Questions
            .GroupBy(q => q.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TopicId, x => x.Count);
        
        foreach (var ta in userTopicAnswers)
        {
            var totalInTopic = topicQuestionTotals.GetValueOrDefault(ta.TopicId, 0);
            var allCorrect = ta.Questions.All(c => c);
            var answeredAll = ta.Questions.Count() == totalInTopic;
            if (totalInTopic > 0 && answeredAll && allCorrect)
                masteredTopicIds.Add(ta.TopicId);
        }

        foreach (var wp in weakProfiles)
        {
            var weakTopicQuery = _db.Topics.Where(t => t.SectionId == wp.SectionId);
            if (filterBySections)
                weakTopicQuery = weakTopicQuery.Where(t => t.SectionId != null && sectionIdSet!.Contains(t.SectionId!.Value));
            var topic = await weakTopicQuery.FirstOrDefaultAsync();

            if (topic != null && masteredTopicIds.Contains(topic.Id))
                continue;

            if (topic != null)
            {
                recs.Add(new RecommendationDto(
                    Type: "daily",
                    Priority: "high",
                    Title: L(lang, $"Подтяните {wp.Section.Name}", $"{wp.Section.Name} жақсартыңыз", $"Improve {wp.Section.Name}"),
                    Description: L(lang,
                        $"Уровень {wp.Level}% — рекомендуем практику по «{topic.Name}»",
                        $"Деңгей {wp.Level}% — «{topic.Name}» бойынша жаттығу ұсынамыз",
                        $"Level {wp.Level}% — we recommend practicing \"{topic.Name}\""),
                    Icon: null,
                    ActionLabel: L(lang, "Практика", "Жаттығу", "Practice"),
                    ActionUrl: $"/learn?tab=practice&topicId={topic.Id}",
                    Metadata: new Dictionary<string, object>
                    {
                        ["sectionId"] = wp.SectionId,
                        ["level"] = wp.Level
                    }
                ));
            }
        }

        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive);

        var hasCompletedMock = await _db.MockExamAttempts
            .AnyAsync(a => a.UserId == userId && a.Status == "Completed");
        if (!hasCompletedMock)
            hasCompletedMock = await _db.TestSessions
                .AnyAsync(s => s.UserId == userId && s.Mode == "exam" && s.CompletedAt != null);

        if (goal != null && !hasCompletedMock)
        {
            var daysUntilExam = (goal.TargetDate - today).TotalDays;
            if (daysUntilExam <= 14 && daysUntilExam > 0)
            {
                recs.Add(new RecommendationDto(
                    Type: "mode",
                    Priority: "high",
                    Title: L(lang,
                        "Экзамен через " + (int)daysUntilExam + " дней!",
                        "Емтиханға " + (int)daysUntilExam + " күн қалды!",
                        "Exam in " + (int)daysUntilExam + " days!"),
                    Description: L(lang,
                        "Попробуйте Mock Exam для тренировки в условиях таймера",
                        "Таймер жағдайында жаттығу үшін Mock Exam қолданып көріңіз",
                        "Try a Mock Exam to practice under timed conditions"),
                    Icon: null,
                    ActionLabel: "Mock Exam",
                    ActionUrl: "/learn?tab=mock",
                    Metadata: new Dictionary<string, object>
                    {
                        ["daysUntilExam"] = (int)daysUntilExam,
                        ["examTypeCode"] = goal.ExamTypeCode
                    }
                ));
            }
            else if (daysUntilExam > 14)
            {
                recs.Add(new RecommendationDto(
                    Type: "mode",
                    Priority: "low",
                    Title: L(lang, "Попробуйте Mock Exam", "Mock Exam қолданып көріңіз", "Try Mock Exam"),
                    Description: L(lang,
                        "Вы ещё не пробовали экзаменационный режим. Потренируйтесь с таймером!",
                        "Сіз әлі емтихан режимін қолданбадыңыз. Таймермен жаттығыңыз!",
                        "You haven't tried exam mode yet. Practice with a timer!"),
                    Icon: null,
                    ActionLabel: L(lang, "Попробовать", "Бастау", "Try it"),
                    ActionUrl: "/learn?tab=mock",
                    Metadata: null
                ));
            }
        }

        var streak = await GetStreakAsync(userId);
        if (streak.CurrentStreak > 0 && !streak.StudiedToday)
        {
            recs.Add(new RecommendationDto(
                Type: "streak",
                Priority: "medium",
                Title: L(lang,
                    $"Серия {streak.CurrentStreak} дней!",
                    $"{streak.CurrentStreak} күн қатарынан!",
                    $"{streak.CurrentStreak}-day streak!"),
                Description: L(lang,
                    "Не прерывайте серию — ответьте хотя бы на 5 вопросов сегодня",
                    "Серияны үзбеңіз — бүгін кемінде 5 сұраққа жауап беріңіз",
                    "Don't break your streak — answer at least 5 questions today"),
                Icon: null,
                ActionLabel: L(lang, "Начать", "Бастау", "Start"),
                ActionUrl: "/learn",
                Metadata: new Dictionary<string, object>
                {
                    ["currentStreak"] = streak.CurrentStreak
                }
            ));
        }

        var priorityOrder = new Dictionary<string, int> { ["high"] = 0, ["medium"] = 1, ["low"] = 2 };
        return recs
            .OrderBy(r => priorityOrder.GetValueOrDefault(r.Priority, 99))
            .Take(8)
            .ToList();
    }


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
