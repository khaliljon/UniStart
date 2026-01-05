using UniStart.DTOs;
using UniStart.Models;

namespace UniStart.Services;

/// <summary>
/// Service for Flashcard business logic
/// </summary>
public interface IFlashcardService
{
    Task<FlashcardSet?> GetSetByIdAsync(int id);
    Task<FlashcardSet?> GetSetWithCardsAsync(int id);
    Task<IEnumerable<FlashcardSet>> GetPublicSetsAsync();
    Task<IEnumerable<FlashcardSet>> GetUserSetsAsync(string userId);
    Task<IEnumerable<FlashcardSet>> GetSetsBySubjectAsync(string subject);
    Task<FlashcardSet> CreateSetAsync(string userId, CreateFlashcardSetDto dto);
    Task<FlashcardSet> UpdateSetAsync(int id, UpdateFlashcardSetDto dto);
    Task<bool> DeleteSetAsync(int id, string userId);
    Task<Flashcard> AddCardToSetAsync(int setId, CreateFlashcardDto dto);
    Task<Flashcard> UpdateCardAsync(int cardId, UpdateFlashcardDto dto);
    Task<bool> DeleteCardAsync(int cardId);
    Task<IEnumerable<Flashcard>> GetCardsDueForReviewAsync(string userId, int setId);
    Task ReviewCardAsync(string userId, int cardId, int quality);
    Task<UserFlashcardProgress?> GetCardProgressAsync(string userId, int cardId);
    Task<Dictionary<string, object>> GetSetStatisticsAsync(string userId, int setId);
}
