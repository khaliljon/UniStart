using UniStart.Models;

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
