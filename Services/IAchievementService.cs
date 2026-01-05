using UniStart.Models;

namespace UniStart.Services;

/// <summary>
/// Service for Achievement business logic
/// </summary>
public interface IAchievementService
{
    Task<IEnumerable<Achievement>> GetAllAchievementsAsync();
    Task<IEnumerable<Achievement>> GetUserAchievementsAsync(string userId);
    Task<IEnumerable<Achievement>> GetUnlockedAchievementsAsync(string userId);
    Task CheckAndUnlockAchievementsAsync(string userId);
    Task<bool> UnlockAchievementAsync(string userId, int achievementId);
    Task<Dictionary<string, int>> GetUserAchievementStatsAsync(string userId);
}
