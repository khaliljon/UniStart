using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class StudyPlanService : IStudyPlanService
{
    private readonly UniStartDbContext _db;
    private readonly ILogger<StudyPlanService> _logger;

    private static readonly int[] ReviewIntervals = { 1, 3, 7, 14, 30 };

    private const int MinDailyMinutes = 30;
    private const int MaxDailyMinutes = 120;
    private const int DefaultTopicMinutes = 20;
    private const int ReviewMinutes = 10;
    private const int PracticeMinutes = 15;

    public StudyPlanService(UniStartDbContext db, ILogger<StudyPlanService> logger)
    {
        _db = db;
        _logger = logger;
    }


    public async Task<StudyGoalDto> CreateGoalAsync(int userId, CreateStudyGoalDto dto)
    {
        var existing = await _db.StudyGoals
            .Where(g => g.UserId == userId && g.IsActive)
            .ToListAsync();

        foreach (var g in existing)
            g.IsActive = false;

        var goal = new StudyGoal
        {
            UserId = userId,
            ExamTypeCode = dto.ExamTypeCode,
            TargetDate = DateTime.SpecifyKind(dto.TargetDate.Date, DateTimeKind.Utc),
            TargetScore = dto.TargetScore,
            SelectedSectionIds = dto.SectionIds != null && dto.SectionIds.Count > 0
                ? string.Join(",", dto.SectionIds)
                : null,
            HoursPerDay = dto.HoursPerDay.HasValue
                ? Math.Round(Math.Clamp(dto.HoursPerDay.Value, 0.5, 6.0), 1)
                : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.StudyGoals.Add(goal);
        await _db.SaveChangesAsync();

        return await MapGoalAsync(goal);
    }

    public async Task<StudyGoalDto?> GetActiveGoalAsync(int userId)
    {
        var goal = await _db.StudyGoals
            .Include(g => g.ExamType)
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive);

        return goal == null ? null : await MapGoalAsync(goal);
    }

    public async Task<StudyGoalDto?> UpdateGoalAsync(int userId, int goalId, UpdateStudyGoalDto dto)
    {
        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId);

        if (goal == null) return null;

        if (dto.TargetDate.HasValue) goal.TargetDate = DateTime.SpecifyKind(dto.TargetDate.Value.Date, DateTimeKind.Utc);
        if (dto.TargetScore.HasValue) goal.TargetScore = dto.TargetScore.Value;
        if (dto.IsActive.HasValue) goal.IsActive = dto.IsActive.Value;

        await _db.SaveChangesAsync();
        return await MapGoalAsync(goal);
    }

    public async Task<bool> DeleteGoalAsync(int userId, int goalId)
    {
        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId);

        if (goal == null) return false;

        goal.IsActive = false;

        var plans = await _db.StudyPlans
            .Where(p => p.GoalId == goalId && p.IsActive)
            .ToListAsync();
        foreach (var p in plans) p.IsActive = false;

        await _db.SaveChangesAsync();
        return true;
    }


    public async Task<StudyPlanDto> GeneratePlanAsync(int userId, int goalId)
    {
        var goal = await _db.StudyGoals
            .Include(g => g.ExamType)
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId)
            ?? throw new InvalidOperationException("Goal not found");

        var oldPlans = await _db.StudyPlans
            .Where(p => p.UserId == userId && p.IsActive)
            .ToListAsync();
        if (oldPlans.Count > 0)
        {
            var oldPlanIds = oldPlans.Select(p => p.Id).ToList();
            var orphanedEntries = await _db.StudyPlanEntries
                .Where(e => oldPlanIds.Contains(e.PlanId))
                .ToListAsync();
            _db.StudyPlanEntries.RemoveRange(orphanedEntries);
            foreach (var p in oldPlans) p.IsActive = false;
        }

        var selectedSectionIds = goal.SelectedSectionIds?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToHashSet();

        var topicQuery = _db.Topics
            .Include(t => t.Section)
            .Where(t => t.Section != null && t.Section.ExamTypeCode == goal.ExamTypeCode);

        if (selectedSectionIds != null && selectedSectionIds.Count > 0)
            topicQuery = topicQuery.Where(t => t.SectionId != null && selectedSectionIds.Contains(t.SectionId.Value));

        var topics = await topicQuery.ToListAsync();

        if (!topics.Any())
        {
            topics = await _db.Topics.Include(t => t.Section).ToListAsync();
        }

        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SectionId, p => p);

        var dependencies = await _db.TopicDependencies.ToListAsync();

        var completedTopicIds = await _db.UserAnswers
            .Where(a => a.UserId == userId)
            .Select(a => a.Question!.TopicId)
            .Distinct()
            .ToListAsync();

        var sortedTopics = TopologicalSortWithPriority(topics, dependencies, profiles);

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var daysUntilExam = (int)(goal.TargetDate.Date - today).TotalDays;
        if (daysUntilExam < 1) daysUntilExam = 7;

        var topicQuestionCounts = await _db.Questions
            .Where(q => topics.Select(t => t.Id).Contains(q.TopicId))
            .GroupBy(q => q.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TopicId, x => x.Count);

        var plan = new StudyPlan
        {
            UserId = userId,
            GoalId = goalId,
            GeneratedAt = DateTime.UtcNow,
            IsActive = true
        };
        _db.StudyPlans.Add(plan);
        await _db.SaveChangesAsync();

        var entries = BuildPlanEntries(
            plan.Id, sortedTopics, profiles, dependencies,
            completedTopicIds, topicQuestionCounts, today, daysUntilExam);

        _db.StudyPlanEntries.AddRange(entries);
        await _db.SaveChangesAsync();

        return await MapPlanAsync(plan.Id);
    }

    public async Task<StudyPlanDto?> GetActivePlanAsync(int userId)
    {
        var plan = await _db.StudyPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        return plan == null ? null : await MapPlanAsync(plan.Id);
    }

    public async Task<StudyPlanDto> RegeneratePlanAsync(int userId)
    {
        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive)
            ?? throw new InvalidOperationException("No active goal found");

        return await GeneratePlanAsync(userId, goal.Id);
    }


    public async Task<TodayPlanDto> GetTodayPlanAsync(int userId)
    {
        var goal = await _db.StudyGoals
            .Include(g => g.ExamType)
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive);

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        var plan = await _db.StudyPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (plan == null || goal == null)
        {
            return new TodayPlanDto(
                Date: today,
                HasGoal: goal != null,
                ExamTypeCode: goal?.ExamTypeCode,
                ExamTypeName: goal?.ExamType?.Name,
                DaysUntilExam: goal != null ? (int)(goal.TargetDate - today).TotalDays : 0,
                TotalMinutesToday: 0,
                Entries: new List<StudyPlanEntryDto>(),
                Recommendation: goal == null
                    ? "Установите цель обучения, чтобы получить персональный план"
                    : "Сгенерируйте план обучения для начала подготовки"
            );
        }

        var entries = await _db.StudyPlanEntries
            .Include(e => e.Topic)
            .Where(e => e.PlanId == plan.Id && e.Date == today)
            .OrderBy(e => e.IsCompleted)
            .ThenBy(e => e.Type == StudyEntryType.Weakness ? 0 :
                         e.Type == StudyEntryType.Review ? 1 :
                         e.Type == StudyEntryType.New ? 2 : 3)
            .ToListAsync();

        var entryTopicIds = entries.Select(e => e.TopicId).Distinct().ToList();
        var topicQCounts = await _db.Questions
            .Where(q => entryTopicIds.Contains(q.TopicId))
            .GroupBy(q => q.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TopicId, x => x.Count);

        foreach (var entry in entries)
        {
            var actual = topicQCounts.GetValueOrDefault(entry.TopicId, entry.RecommendedQuestions);
            if (entry.RecommendedQuestions > actual)
                entry.RecommendedQuestions = actual;
        }

        var totalMinutes = entries.Where(e => !e.IsCompleted).Sum(e => e.RecommendedMinutes);
        var daysUntilExam = (int)(goal.TargetDate - today).TotalDays;

        var completedToday = entries.Count(e => e.IsCompleted);
        var totalToday = entries.Count;

        string recommendation;
        if (totalToday == 0)
            recommendation = "На сегодня заданий нет. Отдохните или займитесь повторением!";
        else if (completedToday == totalToday)
            recommendation = "Все задания на сегодня выполнены! Отличная работа! 🎉";
        else if (entries.Any(e => !e.IsCompleted && e.Type == StudyEntryType.Weakness))
            recommendation = "Сосредоточьтесь на слабых темах — они ускорят ваш прогресс";
        else if (entries.Any(e => !e.IsCompleted && e.Type == StudyEntryType.Review))
            recommendation = "Повторение — мать учения. Начните с Review-задач";
        else
            recommendation = $"У вас {totalToday - completedToday} заданий на сегодня (~{totalMinutes} мин)";

        return new TodayPlanDto(
            Date: today,
            HasGoal: true,
            ExamTypeCode: goal.ExamTypeCode,
            ExamTypeName: goal.ExamType?.Name ?? goal.ExamTypeCode,
            DaysUntilExam: daysUntilExam,
            TotalMinutesToday: totalMinutes,
            Entries: entries.Select(MapEntry).ToList(),
            Recommendation: recommendation
        );
    }


    public async Task<StudyPlanEntryDto?> CompleteEntryAsync(int userId, int entryId, CompleteEntryDto dto)
    {
        var entry = await _db.StudyPlanEntries
            .Include(e => e.Topic)
            .Include(e => e.Plan)
            .FirstOrDefaultAsync(e => e.Id == entryId && e.Plan!.UserId == userId);

        if (entry == null) return null;

        entry.IsCompleted = true;
        entry.CompletedAt = DateTime.UtcNow;
        entry.QuestionsAnswered = dto.QuestionsAnswered;
        entry.CorrectAnswers = dto.CorrectAnswers;

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} completed entry {EntryId}: {Correct}/{Total}",
            userId, entryId, dto.CorrectAnswers, dto.QuestionsAnswered);

        return MapEntry(entry);
    }


    public async Task<TodayPlanDto> AutoCompleteTodayAsync(int userId)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        var plan = await _db.StudyPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (plan == null)
            return await GetTodayPlanAsync(userId);

        var todayEntries = await _db.StudyPlanEntries
            .Include(e => e.Topic)
            .Include(e => e.Plan)
            .Where(e => e.PlanId == plan.Id && e.Date == today)
            .ToListAsync();

        var incompleteEntries = todayEntries.Where(e => !e.IsCompleted).ToList();
        var completedEntries = todayEntries.Where(e => e.IsCompleted).ToList();

        var topicIds = todayEntries.Select(e => e.TopicId).Distinct().ToList();
        var actualTopicCounts = await _db.Questions
            .Where(q => topicIds.Contains(q.TopicId))
            .GroupBy(q => q.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TopicId, x => x.Count);

        foreach (var entry in todayEntries)
        {
            var actual = actualTopicCounts.GetValueOrDefault(entry.TopicId, entry.RecommendedQuestions);
            if (entry.RecommendedQuestions > actual)
                entry.RecommendedQuestions = actual;
        }

        var todayAnswers = await _db.UserAnswers
            .Include(a => a.Question)
            .Include(a => a.AnswerOption)
            .Where(a => a.UserId == userId && a.AnsweredAt >= today)
            .ToListAsync();

        var answersByTopic = todayAnswers
            .Where(a => a.Question != null)
            .GroupBy(a => a.Question!.TopicId)
            .ToDictionary(
                g => g.Key,
                g => new {
                    Total = g.Count(),
                    Correct = g.Count(a => a.AnswerOption != null && a.AnswerOption.IsCorrect)
                }
            );

        var autoCompleted = 0;
        foreach (var entry in incompleteEntries)
        {
            if (!answersByTopic.TryGetValue(entry.TopicId, out var topicStats))
                continue;

            var threshold = Math.Max(2, entry.RecommendedQuestions / 2);
            
            var accuracy = topicStats.Total > 0 ? (double)topicStats.Correct / topicStats.Total : 0;
            var minAccuracy = 0.3;
            
            if (topicStats.Total >= threshold && accuracy >= minAccuracy)
            {
                entry.IsCompleted = true;
                entry.CompletedAt = DateTime.UtcNow;
                entry.QuestionsAnswered = topicStats.Total;
                entry.CorrectAnswers = topicStats.Correct;
                autoCompleted++;
            }
        }

        var statsUpdated = false;
        foreach (var entry in completedEntries)
        {
            if (!answersByTopic.TryGetValue(entry.TopicId, out var freshStats))
                continue;

            if (freshStats.Total != entry.QuestionsAnswered || freshStats.Correct != entry.CorrectAnswers)
            {
                entry.QuestionsAnswered = freshStats.Total;
                entry.CorrectAnswers = freshStats.Correct;
                statsUpdated = true;
            }
        }

        if (autoCompleted > 0 || statsUpdated)
        {
            await _db.SaveChangesAsync();
            if (autoCompleted > 0)
                _logger.LogInformation(
                    "Auto-completed {Count} plan entries for user {UserId}",
                    autoCompleted, userId);
        }

        if (autoCompleted > 0)
            await AdaptPlanAsync(userId, plan.Id, today);

        return await GetTodayPlanAsync(userId);
    }


    private async Task AdaptPlanAsync(int userId, int planId, DateTime today)
    {
        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive);
        if (goal == null) return;

        var allEntries = await _db.StudyPlanEntries
            .Include(e => e.Topic)
            .Where(e => e.PlanId == planId)
            .ToListAsync();

        var futureIncomplete = allEntries
            .Where(e => e.Date > today && !e.IsCompleted)
            .ToList();

        if (!futureIncomplete.Any()) return;

        var completedWithAnswers = allEntries
            .Where(e => e.IsCompleted && e.QuestionsAnswered > 0)
            .ToList();

        var topicAccuracy = completedWithAnswers
            .GroupBy(e => e.TopicId)
            .ToDictionary(
                g => g.Key,
                g => g.Average(e => (double)e.CorrectAnswers / e.QuestionsAnswered));

        var weakTopicIds = topicAccuracy
            .Where(kv => kv.Value < 0.5)
            .Select(kv => kv.Key)
            .ToHashSet();

        var strongTopicIds = topicAccuracy
            .Where(kv => kv.Value >= 0.8)
            .Select(kv => kv.Key)
            .ToHashSet();

        var droppedCount = 0;
        var futureReviews = futureIncomplete
            .Where(e => e.Type == StudyEntryType.Review && strongTopicIds.Contains(e.TopicId))
            .OrderByDescending(e => e.Date)
            .ToList();

        var reviewsByTopic = futureReviews.GroupBy(e => e.TopicId);
        foreach (var group in reviewsByTopic)
        {
            foreach (var extra in group.Skip(1))
            {
                _db.StudyPlanEntries.Remove(extra);
                futureIncomplete.Remove(extra);
                droppedCount++;
            }
        }

        var daysUntilExam = Math.Max(1, (int)(goal.TargetDate.Date - today).TotalDays);
        var remainingDays = daysUntilExam;

        if (futureIncomplete.Any())
        {
            var sorted = futureIncomplete
                .OrderBy(e => e.Type == StudyEntryType.Weakness ? 0 :
                              e.Type == StudyEntryType.New ? 1 :
                              e.Type == StudyEntryType.Review ? 2 : 3)
                .ThenBy(e => weakTopicIds.Contains(e.TopicId) ? 0 : 1)
                .ToList();

            var dailyLoad = new Dictionary<int, int>();
            for (int d = 0; d < remainingDays; d++)
                dailyLoad[d] = 0;

            var targetDaily = Math.Clamp(
                sorted.Sum(e => e.RecommendedMinutes) / Math.Max(remainingDays, 1),
                MinDailyMinutes, MaxDailyMinutes);

            int dayIdx = 0;
            foreach (var entry in sorted)
            {
                var day = FindAvailableDay(dailyLoad, dayIdx, remainingDays, targetDaily);
                entry.Date = DateTime.SpecifyKind(today.AddDays(day + 1).Date, DateTimeKind.Utc);
                dailyLoad[day] += entry.RecommendedMinutes;
                dayIdx = day;
            }
        }

        var topicQCounts = await _db.Questions
            .Where(q => weakTopicIds.Contains(q.TopicId))
            .GroupBy(q => q.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TopicId, x => x.Count);

        var addedCount = 0;
        foreach (var weakId in weakTopicIds)
        {
            var existingFutureCount = futureIncomplete.Count(e => e.TopicId == weakId);
            if (existingFutureCount >= 2) continue;

            var maxQ = topicQCounts.GetValueOrDefault(weakId, 5);
            if (maxQ == 0) continue;

            var targetDay = Math.Min(remainingDays / 3, remainingDays - 1);
            _db.StudyPlanEntries.Add(new StudyPlanEntry
            {
                PlanId = planId,
                TopicId = weakId,
                Date = DateTime.SpecifyKind(today.AddDays(targetDay + 1).Date, DateTimeKind.Utc),
                RecommendedMinutes = (int)(DefaultTopicMinutes * 1.5),
                Type = StudyEntryType.Weakness,
                RecommendedQuestions = Math.Min(8, maxQ)
            });
            addedCount++;
        }

        if (droppedCount > 0 || addedCount > 0)
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "Adapted plan for user {UserId}: dropped {Dropped} strong reviews, added {Added} weakness entries",
                userId, droppedCount, addedCount);
        }
    }


    public async Task<PlanStatsDto> GetPlanStatsAsync(int userId)
    {
        var plan = await _db.StudyPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (plan == null)
        {
            return new PlanStatsDto(0, 0, 0, 0, 0, 0,
                new List<WeekSummaryDto>());
        }

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        var entries = await _db.StudyPlanEntries
            .Where(e => e.PlanId == plan.Id)
            .ToListAsync();

        var pastEntries = entries.Where(e => e.Date <= today).ToList();
        var totalDays = pastEntries.Select(e => e.Date).Distinct().Count();
        var completedDays = pastEntries
            .Where(e => e.IsCompleted)
            .Select(e => e.Date).Distinct().Count();

        var skippedDays = pastEntries
            .GroupBy(e => e.Date)
            .Count(g => g.All(e => !e.IsCompleted) && g.Key < today);

        var completedEntries = entries.Where(e => e.IsCompleted).ToList();
        var avgAccuracy = completedEntries.Any(e => e.QuestionsAnswered > 0)
            ? completedEntries
                .Where(e => e.QuestionsAnswered > 0)
                .Average(e => (double)e.CorrectAnswers / e.QuestionsAnswered * 100)
            : 0;

        var adherence = totalDays > 0
            ? (double)completedDays / totalDays * 100
            : 0;

        var weeklyData = entries
            .GroupBy(e => StartOfWeek(e.Date))
            .OrderBy(g => g.Key)
            .Select(g => new WeekSummaryDto(
                WeekStart: g.Key,
                PlannedEntries: g.Count(),
                CompletedEntries: g.Count(e => e.IsCompleted),
                TotalMinutesPlanned: g.Sum(e => e.RecommendedMinutes),
                Accuracy: g.Any(e => e.IsCompleted && e.QuestionsAnswered > 0)
                    ? Math.Round(g.Where(e => e.IsCompleted && e.QuestionsAnswered > 0)
                        .Average(e => (double)e.CorrectAnswers / e.QuestionsAnswered * 100), 1)
                    : 0
            ))
            .ToList();

        var totalQuestionsAnswered = completedEntries.Sum(e => e.QuestionsAnswered);

        return new PlanStatsDto(
            TotalDays: totalDays,
            CompletedDays: completedDays,
            SkippedDays: skippedDays,
            AverageAccuracy: Math.Round(avgAccuracy, 1),
            TotalQuestionsAnswered: totalQuestionsAnswered,
            AdherencePercent: Math.Round(adherence, 1),
            WeeklySummary: weeklyData
        );
    }


    private List<StudyPlanEntry> BuildPlanEntries(
        int planId,
        List<(Topic topic, double priority)> sortedTopics,
        Dictionary<int, UserSkillProfile> profiles,
        List<TopicDependency> dependencies,
        List<int> completedTopicIds,
        Dictionary<int, int> topicQuestionCounts,
        DateTime startDate,
        int totalDays)
    {
        var entries = new List<StudyPlanEntry>();
        var dailyLoad = new Dictionary<int, int>();

        for (int d = 0; d < totalDays; d++)
            dailyLoad[d] = 0;

        var totalTopics = sortedTopics.Count;
        var targetDailyMinutes = Math.Clamp(
            totalTopics * DefaultTopicMinutes / Math.Max(totalDays, 1),
            MinDailyMinutes, MaxDailyMinutes);

        int dayIndex = 0;
        foreach (var (topic, priority) in sortedTopics)
        {
            var assignDay = FindAvailableDay(dailyLoad, dayIndex, totalDays, targetDailyMinutes);
            if (assignDay >= totalDays) break;

            var skillProfile = profiles.GetValueOrDefault(topic.SectionId ?? 0);
            var theta = skillProfile?.Theta ?? 0.0;
            var isWeak = theta < -0.5;
            var isNew = !completedTopicIds.Contains(topic.Id);

            var type = isNew ? StudyEntryType.New
                     : isWeak ? StudyEntryType.Weakness
                     : StudyEntryType.Practice;

            var minutes = type switch
            {
                StudyEntryType.Weakness => (int)(DefaultTopicMinutes * 1.5),
                StudyEntryType.New => DefaultTopicMinutes,
                _ => PracticeMinutes
            };

            var maxQuestions = topicQuestionCounts.GetValueOrDefault(topic.Id, 5);
            var questions = type switch
            {
                StudyEntryType.Weakness => Math.Min(10, maxQuestions),
                StudyEntryType.New => Math.Min(7, maxQuestions),
                _ => Math.Min(5, maxQuestions)
            };

            entries.Add(new StudyPlanEntry
            {
                PlanId = planId,
                TopicId = topic.Id,
                Date = startDate.AddDays(assignDay),
                RecommendedMinutes = minutes,
                Type = type,
                RecommendedQuestions = questions
            });

            dailyLoad[assignDay] += minutes;
            dayIndex = assignDay;

            foreach (var interval in ReviewIntervals)
            {
                var reviewDay = assignDay + interval;
                if (reviewDay >= totalDays) break;

                var successCount = completedTopicIds.Contains(topic.Id) ? 2 : 0;
                var stability = IrtMath.CalculateStability(successCount);
                var retention = IrtMath.RetentionProbability(interval, stability);

                if (retention >= 0.8) continue;

                var reviewAvailDay = FindAvailableDay(
                    dailyLoad, reviewDay, totalDays, targetDailyMinutes);
                if (reviewAvailDay >= totalDays) break;

                entries.Add(new StudyPlanEntry
                {
                    PlanId = planId,
                    TopicId = topic.Id,
                    Date = startDate.AddDays(reviewAvailDay),
                    RecommendedMinutes = ReviewMinutes,
                    Type = StudyEntryType.Review,
                    RecommendedQuestions = Math.Min(3, topicQuestionCounts.GetValueOrDefault(topic.Id, 3))
                });

                dailyLoad[reviewAvailDay] += ReviewMinutes;
            }
        }

        var weakTopics = sortedTopics
            .Where(t =>
            {
                var p = profiles.GetValueOrDefault(t.topic.SectionId ?? 0);
                return p != null && p.Theta < -0.5;
            })
            .ToList();

        if (weakTopics.Any())
        {
            int weakIdx = 0;
            for (int d = 0; d < totalDays; d++)
            {
                while (dailyLoad[d] < MinDailyMinutes && weakIdx < weakTopics.Count * 3)
                {
                    var (topic, _) = weakTopics[weakIdx % weakTopics.Count];
                    var gapMaxQ = topicQuestionCounts.GetValueOrDefault(topic.Id, 5);

                    entries.Add(new StudyPlanEntry
                    {
                        PlanId = planId,
                        TopicId = topic.Id,
                        Date = startDate.AddDays(d),
                        RecommendedMinutes = PracticeMinutes,
                        Type = StudyEntryType.Weakness,
                        RecommendedQuestions = Math.Min(5, gapMaxQ)
                    });

                    dailyLoad[d] += PracticeMinutes;
                    weakIdx++;
                }
            }
        }

        _logger.LogInformation(
            "Generated plan with {Count} entries over {Days} days",
            entries.Count, totalDays);

        return entries;
    }

    private List<(Topic topic, double priority)> TopologicalSortWithPriority(
        List<Topic> topics,
        List<TopicDependency> dependencies,
        Dictionary<int, UserSkillProfile> profiles)
    {
        var topicIds = new HashSet<int>(topics.Select(t => t.Id));
        var graph = new Dictionary<int, List<int>>();
        var inDegree = new Dictionary<int, int>();

        foreach (var t in topics)
        {
            graph[t.Id] = new List<int>();
            inDegree[t.Id] = 0;
        }

        foreach (var dep in dependencies)
        {
            if (!topicIds.Contains(dep.TopicId) || !topicIds.Contains(dep.PrerequisiteTopicId))
                continue;

            graph[dep.TopicId].Add(dep.PrerequisiteTopicId);
            inDegree[dep.TopicId]++;
        }

        var result = new List<(Topic topic, double priority)>();
        var topicMap = topics.ToDictionary(t => t.Id);

        double GetPriority(Topic t)
        {
            var profile = profiles.GetValueOrDefault(t.SectionId ?? 0);
            var theta = profile?.Theta ?? 0.0;
            return profile == null ? 5.0 : (3.0 - theta);
        }

        var queue = new PriorityQueue<int, double>();
        foreach (var id in topicIds)
        {
            if (inDegree[id] == 0)
                queue.Enqueue(id, -GetPriority(topicMap[id]));
        }

        var visited = new HashSet<int>();
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (!visited.Add(currentId)) continue;

            var topic = topicMap[currentId];
            result.Add((topic, GetPriority(topic)));

            foreach (var dep in dependencies)
            {
                if (dep.PrerequisiteTopicId == currentId && topicIds.Contains(dep.TopicId))
                {
                    inDegree[dep.TopicId]--;
                    if (inDegree[dep.TopicId] <= 0 && !visited.Contains(dep.TopicId))
                    {
                        queue.Enqueue(dep.TopicId, -GetPriority(topicMap[dep.TopicId]));
                    }
                }
            }
        }

        foreach (var t in topics.Where(t => !visited.Contains(t.Id)))
        {
            result.Add((t, GetPriority(t)));
        }

        return result;
    }

    private static int FindAvailableDay(
        Dictionary<int, int> dailyLoad, int startDay, int totalDays, int targetMinutes)
    {
        for (int d = startDay; d < totalDays; d++)
        {
            if (dailyLoad.GetValueOrDefault(d, 0) < targetMinutes)
                return d;
        }

        return Enumerable.Range(0, totalDays)
            .OrderBy(d => dailyLoad.GetValueOrDefault(d, 0))
            .First();
    }


    private async Task<StudyGoalDto> MapGoalAsync(StudyGoal goal)
    {
        if (goal.ExamType == null)
            await _db.Entry(goal).Reference(g => g.ExamType).LoadAsync();

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var daysUntilExam = (int)(goal.TargetDate.Date - today).TotalDays;

        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == goal.UserId)
            .ToListAsync();

        var avgTheta = profiles.Any() ? profiles.Average(p => p.Theta) : 0.0;
        var targetTheta = IrtMath.LevelToTheta(Math.Min(goal.TargetScore, 100));
        var gap = Math.Max(0, targetTheta - avgTheta);

        var totalHoursNeeded = gap * 15;
        var hoursPerDay = goal.HoursPerDay
            ?? (daysUntilExam > 0
                ? Math.Round(Math.Clamp(totalHoursNeeded / daysUntilExam, 0.5, 4.0), 1)
                : 2.0);

        return new StudyGoalDto(
            Id: goal.Id,
            ExamTypeCode: goal.ExamTypeCode,
            ExamTypeName: goal.ExamType?.Name ?? goal.ExamTypeCode,
            TargetDate: goal.TargetDate,
            TargetScore: goal.TargetScore,
            IsActive: goal.IsActive,
            DaysUntilExam: Math.Max(0, daysUntilExam),
            RecommendedHoursPerDay: hoursPerDay,
            CreatedAt: goal.CreatedAt,
            SectionIds: goal.SelectedSectionIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
        );
    }

    private async Task<StudyPlanDto> MapPlanAsync(int planId)
    {
        var plan = await _db.StudyPlans
            .Include(p => p.Goal).ThenInclude(g => g.ExamType)
            .FirstAsync(p => p.Id == planId);

        var entries = await _db.StudyPlanEntries
            .Include(e => e.Topic)
            .Where(e => e.PlanId == planId)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Type)
            .ToListAsync();

        var completed = entries.Count(e => e.IsCompleted);

        var goal = plan.Goal;
        var examTypeCode = goal?.ExamTypeCode ?? "";
        var examTypeName = goal?.ExamType?.Name ?? examTypeCode;

        return new StudyPlanDto(
            Id: plan.Id,
            GoalId: plan.GoalId,
            ExamTypeCode: examTypeCode,
            ExamTypeName: examTypeName,
            GeneratedAt: plan.GeneratedAt,
            IsActive: plan.IsActive,
            TotalEntries: entries.Count,
            CompletedEntries: completed,
            CompletionPercent: entries.Count > 0
                ? Math.Round((double)completed / entries.Count * 100, 1)
                : 0,
            Entries: entries.Select(MapEntry).ToList()
        );
    }

    private static StudyPlanEntryDto MapEntry(StudyPlanEntry e) => new(
        Id: e.Id,
        TopicId: e.TopicId,
        TopicName: e.Topic?.Name ?? "Unknown",
        SectionId: e.Topic?.SectionId,
        Date: e.Date,
        RecommendedMinutes: e.RecommendedMinutes,
        Type: e.Type.ToString(),
        RecommendedQuestions: e.RecommendedQuestions,
        IsCompleted: e.IsCompleted,
        CompletedAt: e.CompletedAt,
        QuestionsAnswered: e.QuestionsAnswered,
        CorrectAnswers: e.CorrectAnswers
    );

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }
}
