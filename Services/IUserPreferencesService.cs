using UniStart.DTOs;
using UniStart.Models.Learning;

namespace UniStart.Services;

/// <summary>
/// Интерфейс сервиса для работы с предпочтениями пользователей
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>
    /// Получить предпочтения пользователя
    /// </summary>
    Task<UserPreferencesResponseDto?> GetUserPreferencesAsync(string userId);
    
    /// <summary>
    /// Создать или обновить предпочтения пользователя
    /// </summary>
    Task<UserPreferencesResponseDto> CreateOrUpdatePreferencesAsync(string userId, UserPreferencesDto dto);
    
    /// <summary>
    /// Проверить, завершил ли пользователь Onboarding
    /// </summary>
    Task<bool> HasCompletedOnboardingAsync(string userId);
    
    /// <summary>
    /// Завершить Onboarding для пользователя
    /// </summary>
    Task<bool> CompleteOnboardingAsync(string userId);
    
    /// <summary>
    /// Получить рекомендуемые предметы на основе целей пользователя
    /// </summary>
    Task<List<SubjectDto>> GetRecommendedSubjectsAsync(string userId);
}
