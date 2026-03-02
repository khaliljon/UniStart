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

    // Spaced repetition intervals (days after initial study)
    private static readonly int[] ReviewIntervals = { 1, 3, 7, 14, 30 };

    // Minutes per session limits
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

    // ═══════════════════════════════════════════════════════
    //  GOALS
    // ═══════════════════════════════════════════════════════

    public async Task<StudyGoalDto> CreateGoalAsync(int userId, CreateStudyGoalDto dto)
    {
        // Deactivate previous goals
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

        // Also deactivate associated plans
        var plans = await _db.StudyPlans
            .Where(p => p.GoalId == goalId && p.IsActive)
            .ToListAsync();
        foreach (var p in plans) p.IsActive = false;

        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════
    //  PLAN GENERATION — Core Algorithm
    // ═══════════════════════════════════════════════════════

    public async Task<StudyPlanDto> GeneratePlanAsync(int userId, int goalId)
    {
        var goal = await _db.StudyGoals
            .Include(g => g.ExamType)
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId)
            ?? throw new InvalidOperationException("Goal not found");

        // Deactivate old plans for this goal
        var oldPlans = await _db.StudyPlans
            .Where(p => p.UserId == userId && p.IsActive)
            .ToListAsync();
        foreach (var p in oldPlans) p.IsActive = false;

        // ─── 1. Load topics for this exam ────────────────────
        var topics = await _db.Topics
            .Include(t => t.Skill)
            .Where(t => t.Section != null && t.Section.ExamTypeCode == goal.ExamTypeCode)
            .ToListAsync();

        if (!topics.Any())
        {
            // Fallback: load all topics if exam-specific filter returns nothing
            topics = await _db.Topics.Include(t => t.Skill).ToListAsync();
        }

        // ─── 2. Load user skill profiles ─────────────────────
        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SkillId, p => p);

        // ─── 3. Load topic dependencies ──────────────────────
        var dependencies = await _db.TopicDependencies.ToListAsync();

        // ─── 4. Load completion history for review scheduling ─
        var completedTopicIds = await _db.UserAnswers
            .Where(a => a.UserId == userId)
            .Select(a => a.Question!.TopicId)
            .Distinct()
            .ToListAsync();

        // ─── 5. Topological sort with priority ───────────────
        var sortedTopics = TopologicalSortWithPriority(topics, dependencies, profiles);

        // ─── 6. Calculate available days ─────────────────────
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var daysUntilExam = (int)(goal.TargetDate.Date - today).TotalDays;
        if (daysUntilExam < 1) daysUntilExam = 7; // At least one week

        // ─── 6b. Get actual question counts per topic ────
        var topicQuestionCounts = await _db.Questions
            .Where(q => topics.Select(t => t.Id).Contains(q.TopicId))
            .GroupBy(q => q.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TopicId, x => x.Count);

        // ─── 7. Build the plan ───────────────────────────────
        var plan = new StudyPlan
        {
            UserId = userId,
            GoalId = goalId,
            GeneratedAt = DateTime.UtcNow,
            IsActive = true
        };
        _db.StudyPlans.Add(plan);
        await _db.SaveChangesAsync(); // Get plan ID

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

    // ═══════════════════════════════════════════════════════
    //  TODAY'S PLAN
    // ═══════════════════════════════════════════════════════

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

        // Cap RecommendedQuestions to actual topic question count
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

    // ═══════════════════════════════════════════════════════
    //  ENTRY COMPLETION
    // ═══════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════
    //  AUTO-COMPLETE TODAY'S ENTRIES FROM ACTUAL ANSWERS
    // ═══════════════════════════════════════════════════════

    public async Task<TodayPlanDto> AutoCompleteTodayAsync(int userId)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        var plan = await _db.StudyPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (plan == null)
            return await GetTodayPlanAsync(userId);

        // Get today's entries (both incomplete for auto-completion and completed for stats refresh)
        var todayEntries = await _db.StudyPlanEntries
            .Include(e => e.Topic)
            .Include(e => e.Plan)
            .Where(e => e.PlanId == plan.Id && e.Date == today)
            .ToListAsync();

        var incompleteEntries = todayEntries.Where(e => !e.IsCompleted).ToList();
        var completedEntries = todayEntries.Where(e => e.IsCompleted).ToList();

        // Fix RecommendedQuestions if they exceed actual topic question count
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

        // Get all user answers from today, grouped by topicId
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

            // Auto-complete if user answered enough questions for this topic today
            // Require at least half the recommended amount (minimum 2) to prevent trivial completion
            var threshold = Math.Max(2, entry.RecommendedQuestions / 2);
            
            // Also require minimum 30% accuracy — answering everything wrong doesn't count
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

        // Also refresh stats for already-completed entries (user may have continued practicing)
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

        return await GetTodayPlanAsync(userId);
    }

    // ═══════════════════════════════════════════════════════
    //  STATS
    // ═══════════════════════════════════════════════════════

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

        // Days with entries where none were completed
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

        // Weekly summary
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

    // ═══════════════════════════════════════════════════════
    //  PLAN GENERATION ALGORITHM
    // ═══════════════════════════════════════════════════════

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
        var dailyLoad = new Dictionary<int, int>(); // dayOffset → minutes

        // Initialize daily load tracker
        for (int d = 0; d < totalDays; d++)
            dailyLoad[d] = 0;

        // Target daily minutes — distribute evenly
        var totalTopics = sortedTopics.Count;
        var targetDailyMinutes = Math.Clamp(
            totalTopics * DefaultTopicMinutes / Math.Max(totalDays, 1),
            MinDailyMinutes, MaxDailyMinutes);

        // ─── Pass 1: Assign main study sessions ─────────
        int dayIndex = 0;
        foreach (var (topic, priority) in sortedTopics)
        {
            // Find the next day that has room
            var assignDay = FindAvailableDay(dailyLoad, dayIndex, totalDays, targetDailyMinutes);
            if (assignDay >= totalDays) break;

            var skillProfile = profiles.GetValueOrDefault(topic.SkillId);
            var theta = skillProfile?.Theta ?? 0.0;
            var isWeak = theta < -0.5;
            var isNew = !completedTopicIds.Contains(topic.Id);

            var type = isNew ? StudyEntryType.New
                     : isWeak ? StudyEntryType.Weakness
                     : StudyEntryType.Practice;

            // Weak topics get more time
            var minutes = type switch
            {
                StudyEntryType.Weakness => (int)(DefaultTopicMinutes * 1.5),
                StudyEntryType.New => DefaultTopicMinutes,
                _ => PracticeMinutes
            };

            // More questions for weak topics, but never more than available in the topic
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
            dayIndex = assignDay; // Next topic starts from this day or later

            // ─── Pass 2: Schedule reviews using Ebbinghaus curve ───
            foreach (var interval in ReviewIntervals)
            {
                var reviewDay = assignDay + interval;
                if (reviewDay >= totalDays) break;

                // Check retention — skip review if retention is still high
                var successCount = completedTopicIds.Contains(topic.Id) ? 2 : 0;
                var stability = IrtMath.CalculateStability(successCount);
                var retention = IrtMath.RetentionProbability(interval, stability);

                // Only schedule review if retention drops below 80%
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

        // ─── Pass 3: Fill gaps with weakness practice ────
        var weakTopics = sortedTopics
            .Where(t =>
            {
                var p = profiles.GetValueOrDefault(t.topic.SkillId);
                return p != null && p.Theta < -0.5;
            })
            .ToList();

        if (weakTopics.Any())
        {
            int weakIdx = 0;
            for (int d = 0; d < totalDays; d++)
            {
                // Fill days with less than minimum load
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

    /// <summary>
    /// Topologically sort topics respecting dependencies, then by priority (weak first).
    /// </summary>
    private List<(Topic topic, double priority)> TopologicalSortWithPriority(
        List<Topic> topics,
        List<TopicDependency> dependencies,
        Dictionary<int, UserSkillProfile> profiles)
    {
        var topicIds = new HashSet<int>(topics.Select(t => t.Id));
        var graph = new Dictionary<int, List<int>>(); // topic → prerequisite topic IDs
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

        // Kahn's algorithm with priority queue (higher priority = earlier)
        var result = new List<(Topic topic, double priority)>();
        var topicMap = topics.ToDictionary(t => t.Id);

        // Priority: lower theta = higher priority (study weak topics first)
        double GetPriority(Topic t)
        {
            var profile = profiles.GetValueOrDefault(t.SkillId);
            var theta = profile?.Theta ?? 0.0;
            // Invert theta so weak topics (negative θ) get high priority
            // Also factor in: topics without profile data get medium-high priority
            return profile == null ? 5.0 : (3.0 - theta);
        }

        // Start with topics that have no prerequisites
        var queue = new PriorityQueue<int, double>();
        foreach (var id in topicIds)
        {
            if (inDegree[id] == 0)
                queue.Enqueue(id, -GetPriority(topicMap[id])); // Negate for max-heap behavior
        }

        var visited = new HashSet<int>();
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (!visited.Add(currentId)) continue;

            var topic = topicMap[currentId];
            result.Add((topic, GetPriority(topic)));

            // Release dependent topics
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

        // Add any remaining topics (cycles or disconnected)
        foreach (var t in topics.Where(t => !visited.Contains(t.Id)))
        {
            result.Add((t, GetPriority(t)));
        }

        return result;
    }

    /// <summary>
    /// Find the next available day that hasn't exceeded the daily minutes target.
    /// </summary>
    private static int FindAvailableDay(
        Dictionary<int, int> dailyLoad, int startDay, int totalDays, int targetMinutes)
    {
        // First try to find a day from startDay that has room
        for (int d = startDay; d < totalDays; d++)
        {
            if (dailyLoad.GetValueOrDefault(d, 0) < targetMinutes)
                return d;
        }

        // If all days are full, find the least loaded day from startDay
        return Enumerable.Range(startDay, Math.Max(1, totalDays - startDay))
            .OrderBy(d => dailyLoad.GetValueOrDefault(d, 0))
            .FirstOrDefault(startDay);
    }

    // ═══════════════════════════════════════════════════════
    //  MAPPERS
    // ═══════════════════════════════════════════════════════

    private async Task<StudyGoalDto> MapGoalAsync(StudyGoal goal)
    {
        if (goal.ExamType == null)
            await _db.Entry(goal).Reference(g => g.ExamType).LoadAsync();

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var daysUntilExam = (int)(goal.TargetDate.Date - today).TotalDays;

        // Estimate recommended hours per day
        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == goal.UserId)
            .ToListAsync();

        var avgTheta = profiles.Any() ? profiles.Average(p => p.Theta) : 0.0;
        var targetTheta = IrtMath.LevelToTheta(Math.Min(goal.TargetScore, 100));
        var gap = Math.Max(0, targetTheta - avgTheta);

        // More gap = more hours needed; distribute over remaining days
        var totalHoursNeeded = gap * 15; // rough: ~15 hours per θ unit gap
        var hoursPerDay = daysUntilExam > 0
            ? Math.Round(Math.Clamp(totalHoursNeeded / daysUntilExam, 0.5, 4.0), 1)
            : 2.0;

        return new StudyGoalDto(
            Id: goal.Id,
            ExamTypeCode: goal.ExamTypeCode,
            ExamTypeName: goal.ExamType?.Name ?? goal.ExamTypeCode,
            TargetDate: goal.TargetDate,
            TargetScore: goal.TargetScore,
            IsActive: goal.IsActive,
            DaysUntilExam: Math.Max(0, daysUntilExam),
            RecommendedHoursPerDay: hoursPerDay,
            CreatedAt: goal.CreatedAt
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
