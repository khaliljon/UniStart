using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationPreferencesDto> GetPreferencesAsync(int userId);
    Task<NotificationPreferencesDto> UpdatePreferencesAsync(int userId, UpdateNotificationPreferencesDto dto);
    Task EnsurePreferencesExistAsync(int userId);
}
