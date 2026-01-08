using UniStart.Models.Learning;
using UniStart.Models.Reference;

namespace UniStart.Services.AI;

/// <summary>
/// Интерфейс для рекомендательной системы университетов
/// </summary>
public interface IUniversityRecommendationService
{
    /// <summary>
    /// Получить топ-N рекомендаций университетов для пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="topN">Количество рекомендаций (по умолчанию 10)</param>
    /// <param name="forceRefresh">Принудительно пересчитать рекомендации</param>
    /// <returns>Список рекомендаций с оценками</returns>
    Task<List<UniversityRecommendation>> GetRecommendations(string userId, int topN = 10, bool forceRefresh = false);
    
    /// <summary>
    /// Построить профиль пользователя на основе его прогресса
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <returns>Профиль пользователя</returns>
    Task<UserProfile?> BuildUserProfile(string userId);
    
    /// <summary>
    /// Обновить предпочтения пользователя (город, бюджет, цель)
    /// </summary>
    Task<bool> UpdateUserPreferences(string userId, string? preferredCity, decimal? maxBudget, string? careerGoal);
    
    /// <summary>
    /// Отметить рекомендацию как просмотренную
    /// </summary>
    Task MarkAsViewed(int recommendationId);
    
    /// <summary>
    /// Добавить оценку пользователя к рекомендации (обратная связь для улучшения алгоритма)
    /// </summary>
    Task RateRecommendation(int recommendationId, int rating);
}
