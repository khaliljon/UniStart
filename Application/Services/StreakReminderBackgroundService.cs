using Microsoft.EntityFrameworkCore;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Background job: sends streak reminder emails to users who haven't studied for 2+ days.
/// Runs every 6 hours.
/// </summary>
public class StreakReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StreakReminderBackgroundService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    public StreakReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<StreakReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StreakReminderBackgroundService started.");

        // Wait 1 minute on startup so DB is ready
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessStreakRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing streak reminders.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessStreakRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UniStartDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        // Get users with streak reminder enabled who haven't been reminded in the last 24h
        var users = await context.Users
            .Include(u => u.NotificationPreferences)
            .Where(u => u.NotificationPreferences != null
                        && u.NotificationPreferences.StreakReminder
                        && (u.NotificationPreferences.LastStreakReminderSentAt == null
                            || u.NotificationPreferences.LastStreakReminderSentAt < oneDayAgo))
            .ToListAsync(ct);

        foreach (var user in users)
        {
            // Check if user has any answers in the last 2 days
            var hasRecentActivity = await context.UserAnswers
                .AnyAsync(a => a.UserId == user.Id && a.AnsweredAt > twoDaysAgo, ct);

            if (hasRecentActivity) continue;

            // Calculate their last streak
            var lastAnswerDate = await context.UserAnswers
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.AnsweredAt)
                .Select(a => a.AnsweredAt)
                .FirstOrDefaultAsync(ct);

            var streak = 0;
            if (lastAnswerDate != default)
            {
                // Count consecutive days back from last answer
                var checkDate = lastAnswerDate.Date;
                while (true)
                {
                    var hasActivity = await context.UserAnswers
                        .AnyAsync(a => a.UserId == user.Id
                                       && a.AnsweredAt.Date == checkDate, ct);
                    if (!hasActivity) break;
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }
            }

            await emailService.SendStreakReminderAsync(user.Email, user.Name, streak);

            // Update last sent timestamp
            user.NotificationPreferences!.LastStreakReminderSentAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Streak reminders processed for {Count} candidate users.", users.Count);
    }
}
