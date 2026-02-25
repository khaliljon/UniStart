using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class NotificationService : INotificationService
{
    private readonly UniStartDbContext _context;

    public NotificationService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferencesDto> GetPreferencesAsync(int userId)
    {
        var prefs = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (prefs == null)
        {
            prefs = await CreateDefaultPreferencesAsync(userId);
        }

        return MapToDto(prefs);
    }

    public async Task<NotificationPreferencesDto> UpdatePreferencesAsync(int userId, UpdateNotificationPreferencesDto dto)
    {
        var prefs = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (prefs == null)
        {
            prefs = await CreateDefaultPreferencesAsync(userId);
        }

        if (dto.WelcomeEmail.HasValue) prefs.WelcomeEmail = dto.WelcomeEmail.Value;
        if (dto.StreakReminder.HasValue) prefs.StreakReminder = dto.StreakReminder.Value;
        if (dto.WeeklyDigest.HasValue) prefs.WeeklyDigest = dto.WeeklyDigest.Value;
        if (dto.StudyPlanReminder.HasValue) prefs.StudyPlanReminder = dto.StudyPlanReminder.Value;
        if (dto.AchievementNotification.HasValue) prefs.AchievementNotification = dto.AchievementNotification.Value;

        prefs.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(prefs);
    }

    public async Task EnsurePreferencesExistAsync(int userId)
    {
        var exists = await _context.NotificationPreferences
            .AnyAsync(p => p.UserId == userId);

        if (!exists)
        {
            await CreateDefaultPreferencesAsync(userId);
        }
    }

    private async Task<NotificationPreferences> CreateDefaultPreferencesAsync(int userId)
    {
        var prefs = new NotificationPreferences
        {
            UserId = userId,
            WelcomeEmail = true,
            StreakReminder = true,
            WeeklyDigest = true,
            StudyPlanReminder = true,
            AchievementNotification = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.NotificationPreferences.Add(prefs);
        await _context.SaveChangesAsync();
        return prefs;
    }

    private static NotificationPreferencesDto MapToDto(NotificationPreferences prefs)
    {
        return new NotificationPreferencesDto(
            prefs.WelcomeEmail,
            prefs.StreakReminder,
            prefs.WeeklyDigest,
            prefs.StudyPlanReminder,
            prefs.AchievementNotification
        );
    }
}
