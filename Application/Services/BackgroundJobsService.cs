using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Hangfire-managed background jobs: streak reminders and weekly digests.
/// Each method is called by Hangfire as a recurring job with automatic retry.
/// </summary>
public class BackgroundJobsService : IBackgroundJobsService
{
    private readonly UniStartDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<BackgroundJobsService> _logger;

    public BackgroundJobsService(
        UniStartDbContext context,
        IEmailService emailService,
        ILogger<BackgroundJobsService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    // ───────────────────────────────────────────────────────
    //  STREAK REMINDERS (every 6 hours)
    // ───────────────────────────────────────────────────────
    public async Task ProcessStreakRemindersAsync()
    {
        _logger.LogInformation("Hangfire: Processing streak reminders...");

        var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        var users = await _context.Users
            .Include(u => u.NotificationPreferences)
            .Where(u => u.NotificationPreferences != null
                        && u.NotificationPreferences.StreakReminder
                        && (u.NotificationPreferences.LastStreakReminderSentAt == null
                            || u.NotificationPreferences.LastStreakReminderSentAt < oneDayAgo))
            .ToListAsync();

        var sentCount = 0;

        foreach (var user in users)
        {
            var hasRecentActivity = await _context.UserAnswers
                .AnyAsync(a => a.UserId == user.Id && a.AnsweredAt > twoDaysAgo);

            if (hasRecentActivity) continue;

            var lastAnswerDate = await _context.UserAnswers
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.AnsweredAt)
                .Select(a => a.AnsweredAt)
                .FirstOrDefaultAsync();

            var streak = 0;
            if (lastAnswerDate != default)
            {
                var checkDate = lastAnswerDate.Date;
                while (true)
                {
                    var hasActivity = await _context.UserAnswers
                        .AnyAsync(a => a.UserId == user.Id && a.AnsweredAt.Date == checkDate);
                    if (!hasActivity) break;
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }
            }

            // Calculate actual days of inactivity
            var inactiveDays = lastAnswerDate != default
                ? (int)(DateTime.UtcNow.Date - lastAnswerDate.Date).TotalDays
                : 2; // default if no activity ever

            try
            {
                await _emailService.SendStreakReminderAsync(user.Email, user.Name, streak, inactiveDays);
                user.NotificationPreferences!.LastStreakReminderSentAt = DateTime.UtcNow;
                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send streak reminder to {Email}", user.Email);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Streak reminders sent to {Count} users.", sentCount);
    }

    // ───────────────────────────────────────────────────────
    //  WEEKLY DIGESTS (Mondays 08:00 UTC)
    // ───────────────────────────────────────────────────────
    public async Task ProcessWeeklyDigestsAsync()
    {
        _logger.LogInformation("Hangfire: Processing weekly digests...");

        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        var users = await _context.Users
            .Include(u => u.NotificationPreferences)
            .Where(u => u.NotificationPreferences != null
                        && u.NotificationPreferences.WeeklyDigest
                        && (u.NotificationPreferences.LastWeeklyDigestSentAt == null
                            || u.NotificationPreferences.LastWeeklyDigestSentAt < oneDayAgo))
            .ToListAsync();

        var sentCount = 0;

        foreach (var user in users)
        {
            var weeklyAnswers = await _context.UserAnswers
                .Include(a => a.AnswerOption)
                .Include(a => a.Question)
                    .ThenInclude(q => q.Topic)
                        .ThenInclude(t => t.Skill)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1)
                .ToListAsync();

            if (!weeklyAnswers.Any()) continue;

            var totalQuestions = weeklyAnswers.Count;
            var correctAnswers = weeklyAnswers.Count(a => a.AnswerOption.IsCorrect);
            var accuracy = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;

            // Calculate streak
            var streak = 0;
            var checkDate = DateTime.UtcNow.Date.AddDays(-1);
            while (true)
            {
                var hasActivity = await _context.UserAnswers
                    .AnyAsync(a => a.UserId == user.Id
                                   && a.AnsweredAt.Date == checkDate
                                   && a.TimeSpentSeconds != -1);
                if (!hasActivity) break;
                streak++;
                checkDate = checkDate.AddDays(-1);
            }

            // Get exam type — the most used one
            var examTypeCode = await _context.UserAnswers
                .Include(a => a.Question).ThenInclude(q => q.Topic).ThenInclude(t => t.Section)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1
                            && a.Question.Topic.Section != null)
                .Select(a => a.Question.Topic.Section!.ExamTypeCode)
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync() ?? "SAT";

            var examType = await _context.ExamTypes.FindAsync(examTypeCode);

            // Topic progress
            var topProgress = weeklyAnswers
                .Where(a => a.Question?.Topic != null)
                .GroupBy(a => a.Question.Topic.Name)
                .Select(g => new WeeklyTopicProgressDto(
                    g.Key,
                    g.Count(),
                    g.Count(a => a.AnswerOption.IsCorrect) * 100.0 / g.Count(),
                    g.Count(a => a.AnswerOption.IsCorrect) * 100.0 / g.Count() > 70 ? "improving" :
                    g.Count(a => a.AnswerOption.IsCorrect) * 100.0 / g.Count() < 40 ? "declining" : "stable"
                ))
                .OrderByDescending(t => t.QuestionsAnswered)
                .Take(5)
                .ToList();

            // Predicted score
            var predictedScore = 0;
            var maxScore = 1600;
            try
            {
                var sections = await _context.ExamSections
                    .Where(s => s.ExamTypeCode == examTypeCode)
                    .ToListAsync();
                maxScore = sections.Sum(s => s.MaxScore);
                var minScore = sections.Sum(s => s.MinScore);

                var sectionIds = sections.Select(s => s.Id).ToList();
                var relevantSkillIds = await _context.Topics
                    .Where(t => t.SectionId != null && sectionIds.Contains(t.SectionId.Value))
                    .Select(t => t.SkillId)
                    .Distinct()
                    .ToListAsync();

                var profiles = await _context.UserSkillProfiles
                    .Where(p => p.UserId == user.Id && relevantSkillIds.Contains(p.SkillId))
                    .ToListAsync();

                if (profiles.Any())
                {
                    var avgTheta = profiles.Average(p => p.Theta);
                    var probability = 1.0 / (1.0 + Math.Exp(-1.7 * avgTheta));
                    predictedScore = (int)(minScore + probability * (maxScore - minScore));
                }
            }
            catch { /* fallback to 0 */ }

            var recommendations = new List<string>();
            if (accuracy < 50)
                recommendations.Add("Сфокусируйтесь на слабых темах — повторите материал по ним.");
            if (totalQuestions < 30)
                recommendations.Add("Попробуйте решать больше вопросов — минимум 5 в день.");
            if (streak == 0)
                recommendations.Add("Начните заниматься ежедневно — постоянство даёт результат!");
            if (accuracy > 70)
                recommendations.Add("Отличный прогресс! Попробуйте более сложные вопросы.");

            var digestData = new WeeklyDigestDataDto(
                user.Name,
                totalQuestions,
                correctAnswers,
                accuracy,
                streak,
                predictedScore,
                maxScore,
                examType?.Name ?? examTypeCode,
                topProgress,
                recommendations
            );

            try
            {
                await _emailService.SendWeeklyDigestAsync(user.Email, digestData);
                user.NotificationPreferences!.LastWeeklyDigestSentAt = DateTime.UtcNow;
                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send weekly digest to {Email}", user.Email);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Weekly digests sent to {Count} users.", sentCount);
    }
}
