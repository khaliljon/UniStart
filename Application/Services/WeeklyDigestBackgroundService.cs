using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

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
            var weeklyAnswers = await context.UserAnswers
                .Include(a => a.AnswerOption)
                .Include(a => a.Question)
                    .ThenInclude(q => q.Topic)
                        .ThenInclude(t => t.Section)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1)
                .ToListAsync(ct);

            if (!weeklyAnswers.Any()) continue;

            var totalQuestions = weeklyAnswers.Count;
            var correctAnswers = weeklyAnswers.Count(a => a.AnswerOption.IsCorrect);
            var accuracy = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;

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

            var examTypeCode = await context.UserAnswers
                .Include(a => a.Question).ThenInclude(q => q.Topic).ThenInclude(t => t.Section)
                .Where(a => a.UserId == user.Id && a.AnsweredAt > oneWeekAgo && a.TimeSpentSeconds != -1
                            && a.Question.Topic.Section != null)
                .Select(a => a.Question.Topic.Section!.ExamTypeCode)
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync(ct) ?? "CSCA";

            var examType = await context.ExamTypes.FindAsync(new object[] { examTypeCode }, ct);

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
                var sections = await context.ExamSections
                    .Where(s => s.ExamTypeCode == examTypeCode)
                    .ToListAsync(ct);
                maxScore = sections.Sum(s => s.MaxScore);
                var minScore = sections.Sum(s => s.MinScore);

                var sectionIds = sections.Select(s => s.Id).ToList();
                var profiles = await context.UserSkillProfiles
                    .Where(p => p.UserId == user.Id && sectionIds.Contains(p.SectionId))
                    .ToListAsync(ct);

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

            await emailService.SendWeeklyDigestAsync(user.Email, digestData);

            user.NotificationPreferences!.LastWeeklyDigestSentAt = DateTime.UtcNow;
            sentCount++;
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Weekly digests sent to {Count} users.", sentCount);
    }
}
