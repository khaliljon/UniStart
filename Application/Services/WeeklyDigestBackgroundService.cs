using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Background job: sends weekly digest emails every Monday at ~8:00 UTC.
/// </summary>
public class WeeklyDigestBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeeklyDigestBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public WeeklyDigestBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WeeklyDigestBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WeeklyDigestBackgroundService started.");
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                // Send on Mondays between 8:00-9:00 UTC
                if (now.DayOfWeek == DayOfWeek.Monday && now.Hour is >= 8 and < 9)
                {
                    await ProcessWeeklyDigestsAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weekly digests.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessWeeklyDigestsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UniStartDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        // Get users with weekly digest enabled who haven't received one in the last day
        var users = await context.Users
            .Include(u => u.NotificationPreferences)
            .Where(u => u.NotificationPreferences != null
                        && u.NotificationPreferences.WeeklyDigest
                        && (u.NotificationPreferences.LastWeeklyDigestSentAt == null
                            || u.NotificationPreferences.LastWeeklyDigestSentAt < oneDayAgo))
            .ToListAsync(ct);

        var sentCount = 0;

        foreach (var user in users)
        {
            // Get weekly answers
            var weeklyAnswers = await context.UserAnswers
                .Include(a => a.AnswerOption)
                .Include(a => a.Question)
                    .ThenInclude(q => q.Topic)
                        .ThenInclude(t => t.Skill)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1)
                .ToListAsync(ct);

            if (!weeklyAnswers.Any()) continue;

            var totalQuestions = weeklyAnswers.Count;
            var correctAnswers = weeklyAnswers.Count(a => a.AnswerOption.IsCorrect);
            var accuracy = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;

            // Calculate streak
            var streak = 0;
            var checkDate = DateTime.UtcNow.Date.AddDays(-1);
            while (true)
            {
                var hasActivity = await context.UserAnswers
                    .AnyAsync(a => a.UserId == user.Id
                                   && a.AnsweredAt.Date == checkDate
                                   && a.TimeSpentSeconds != -1, ct);
                if (!hasActivity) break;
                streak++;
                checkDate = checkDate.AddDays(-1);
            }

            // Get exam type — find the most used one
            var examTypeCode = await context.UserAnswers
                .Include(a => a.Question).ThenInclude(q => q.Topic).ThenInclude(t => t.Section)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1
                            && a.Question.Topic.Section != null)
                .Select(a => a.Question.Topic.Section!.ExamTypeCode)
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync(ct) ?? "SAT";

            var examType = await context.ExamTypes.FindAsync(new object[] { examTypeCode }, ct);

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

            // Simple predicted score (use skill profiles)
            var predictedScore = 0;
            var maxScore = 1600;
            try
            {
                // Get sections for the exam and their score ranges
                var sections = await context.ExamSections
                    .Where(s => s.ExamTypeCode == examTypeCode)
                    .ToListAsync(ct);
                maxScore = sections.Sum(s => s.MaxScore);
                var minScore = sections.Sum(s => s.MinScore);

                // Get user skill profiles for skills that have topics in this exam's sections
                var sectionIds = sections.Select(s => s.Id).ToList();
                var relevantSkillIds = await context.Topics
                    .Where(t => t.SectionId != null && sectionIds.Contains(t.SectionId.Value))
                    .Select(t => t.SkillId)
                    .Distinct()
                    .ToListAsync(ct);

                var profiles = await context.UserSkillProfiles
                    .Where(p => p.UserId == user.Id && relevantSkillIds.Contains(p.SkillId))
                    .ToListAsync(ct);

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

            await emailService.SendWeeklyDigestAsync(user.Email, digestData);

            user.NotificationPreferences!.LastWeeklyDigestSentAt = DateTime.UtcNow;
            sentCount++;
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Weekly digests sent to {Count} users.", sentCount);
    }
}
