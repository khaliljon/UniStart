using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Repositories;

/// <summary>
/// Repository for FlashcardSet entity with specific operations
/// </summary>
public interface IFlashcardSetRepository : IRepository<FlashcardSet>
{
    Task<IEnumerable<FlashcardSet>> GetPublicSetsAsync();
    Task<IEnumerable<FlashcardSet>> GetSetsByUserAsync(string userId);
    Task<IEnumerable<FlashcardSet>> GetSetsBySubjectAsync(string subject);
    Task<FlashcardSet?> GetSetWithFlashcardsAsync(int setId);
    Task<FlashcardSet?> GetSetWithProgressAsync(int setId, string userId);
}
