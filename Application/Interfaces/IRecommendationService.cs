using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IRecommendationService
{
    Task<DailyBriefingDto> GetDailyBriefingAsync(int userId, List<int>? sectionIds = null);

    Task<AfterSessionDto> GetAfterSessionRecommendationsAsync(int userId, int sessionId);

    Task<StreakDto> GetStreakAsync(int userId);

    Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int userId);

    Task<IEnumerable<MilestoneDto>> CheckAndAwardMilestonesAsync(int userId);
}
