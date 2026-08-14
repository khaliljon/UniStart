using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

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

            if (lastAnswerDate == default) continue;

            var streak = 0;
            if (lastAnswerDate != default)
            {
                var studyDates = await _context.UserAnswers
                    .Where(a => a.UserId == user.Id)
                    .Select(a => a.AnsweredAt.Date)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToListAsync();

                var checkDate = lastAnswerDate.Date;
                foreach (var day in studyDates)
                {
                    if (day == checkDate)
                    {
                        streak++;
                        checkDate = checkDate.AddDays(-1);
                    }
                    else if (day < checkDate)
                    {
                        break;
                    }
                }
            }

            var inactiveDays = (int)(DateTime.UtcNow.Date - lastAnswerDate.Date).TotalDays;

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
                        .ThenInclude(t => t.Section)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1)
                .ToListAsync();

            if (!weeklyAnswers.Any()) continue;

            var totalQuestions = weeklyAnswers.Count;
            var correctAnswers = weeklyAnswers.Count(a => a.AnswerOption.IsCorrect);
            var accuracy = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;

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

            var examTypeCode = await _context.UserAnswers
                .Include(a => a.Question).ThenInclude(q => q.Topic).ThenInclude(t => t.Section)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1
                            && a.Question.Topic.Section != null)
                .Select(a => a.Question.Topic.Section!.ExamTypeCode)
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync() ?? "CSCA";

            var examType = await _context.ExamTypes.FindAsync(examTypeCode);

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
                var profiles = await _context.UserSkillProfiles
                    .Where(p => p.UserId == user.Id && sectionIds.Contains(p.SectionId))
                    .ToListAsync();

                if (profiles.Any())
                {
                    var avgTheta = profiles.Average(p => p.Theta);
                    var probability = 1.0 / (1.0 + Math.Exp(-1.7 * avgTheta));
                    predictedScore = (int)(minScore + probability * (maxScore - minScore));
                }
            }
            catch {}

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

    public async Task PurgeSoftDeletedRecordsAsync()
    {
        _logger.LogInformation("Hangfire: Purging soft-deleted records older than 30 days...");

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var totalPurged = 0;

        var deletedQuestions = await _context.Questions
            .IgnoreQueryFilters()
            .Where(q => q.IsDeleted && q.DeletedAt != null && q.DeletedAt < cutoff)
            .ToListAsync();

        if (deletedQuestions.Any())
        {
            _context.Questions.RemoveRange(deletedQuestions);
            totalPurged += deletedQuestions.Count;
            _logger.LogInformation("Purging {Count} soft-deleted questions.", deletedQuestions.Count);
        }

        var deletedUsers = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsDeleted && u.DeletedAt != null && u.DeletedAt < cutoff)
            .ToListAsync();

        if (deletedUsers.Any())
        {
            _context.Users.RemoveRange(deletedUsers);
            totalPurged += deletedUsers.Count;
            _logger.LogInformation("Purging {Count} soft-deleted users.", deletedUsers.Count);
        }

        if (totalPurged > 0)
        {
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Soft-delete purge complete. {Count} records permanently removed.", totalPurged);
    }

    public async Task CalibrateIrtParametersAsync()
    {
        _logger.LogInformation("Hangfire: Starting IRT auto-calibration...");

        const int MinAnswers = 30;
        const double MinDelta = 0.15;

        var stats = await _context.UserAnswers
            .Where(ua => ua.AnswerOption != null)
            .GroupBy(ua => ua.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                Total = g.Count(),
                Correct = g.Count(ua => ua.AnswerOption!.IsCorrect)
            })
            .Where(s => s.Total >= MinAnswers)
            .ToListAsync();

        if (stats.Count == 0)
        {
            _logger.LogInformation("IRT calibration: no questions with {Min}+ answers yet.", MinAnswers);
            return;
        }

        var questionIds = stats.Select(s => s.QuestionId).ToList();
        var questions = await _context.Questions
            .Where(q => questionIds.Contains(q.Id) && !q.IsDeleted)
            .ToListAsync();

        var updated = 0;
        foreach (var q in questions)
        {
            var s = stats.First(x => x.QuestionId == q.Id);
            if (s.Total == 0) continue;

            double p = (double)s.Correct / s.Total;
            p = Math.Clamp(p, 0.01, 0.99);

            double bNew = Math.Log((1.0 - p) / p);

            if (Math.Abs(bNew - q.DifficultyParam) > MinDelta)
            {
                q.DifficultyParam = Math.Round(Math.Clamp(bNew, -4.0, 4.0), 3);
                q.UpdatedAt = DateTime.UtcNow;
                updated++;
            }
        }

        if (updated > 0)
            await _context.SaveChangesAsync();

        _logger.LogInformation(
            "IRT calibration complete. {Updated} of {Checked} questions updated.",
            updated, questions.Count);
    }
}
