using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Repositories;

/// <summary>
/// Repository for UserFlashcardProgress entity with specific operations
/// </summary>
public interface IUserFlashcardProgressRepository : IRepository<UserFlashcardProgress>
{
    Task<IEnumerable<UserFlashcardProgress>> GetUserProgressAsync(string userId);
    Task<IEnumerable<UserFlashcardProgress>> GetProgressForSetAsync(string userId, int setId);
    Task<UserFlashcardProgress?> GetProgressAsync(string userId, int flashcardId);
    Task<int> GetMasteredCardsCountAsync(string userId);
    Task<IEnumerable<UserFlashcardProgress>> GetCardsDueForReviewAsync(string userId);
}
