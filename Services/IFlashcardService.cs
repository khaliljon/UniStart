using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

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
    Task<IEnumerable<FlashcardSet>> GetSetsBySubjectsAsync(List<int> subjectIds);
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
