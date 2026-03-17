using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IRecommendationService
{
    /// <summary>
    /// Ежедневный брифинг: стрик, рекомендации, последние достижения
    /// </summary>
    Task<DailyBriefingDto> GetDailyBriefingAsync(int userId, List<int>? sectionIds = null);

    /// <summary>
    /// Рекомендации после завершения тестовой сессии
    /// </summary>
    Task<AfterSessionDto> GetAfterSessionRecommendationsAsync(int userId, int sessionId);

    /// <summary>
    /// Получить данные о стрике
    /// </summary>
    Task<StreakDto> GetStreakAsync(int userId);

    /// <summary>
    /// Все достижения (milestones) пользователя
    /// </summary>
    Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int userId);

    /// <summary>
    /// Проверить и начислить новые достижения (вызывается после ответа)
    /// </summary>
    Task<IEnumerable<MilestoneDto>> CheckAndAwardMilestonesAsync(int userId);
}
