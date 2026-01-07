using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

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
