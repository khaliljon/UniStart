using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository for Achievement entity with specific operations
/// </summary>
public interface IAchievementRepository : IRepository<Achievement>
{
    Task<IEnumerable<Achievement>> GetAchievementsByTypeAsync(string type);
    Task<IEnumerable<Achievement>> GetUserAchievementsAsync(string userId);
    Task<int> GetUnlockedCountAsync(string userId);
}
