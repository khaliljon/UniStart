using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

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
