using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IFlashcardService
{
    Task<IEnumerable<FlashcardDeckDto>> GetDecksAsync(int userId, string[]? examTypeCodes = null);
    Task<IEnumerable<FlashcardReviewDto>> GetDueCardsAsync(int userId, int deckId, int limit = 20);
    Task ReviewCardAsync(int userId, ReviewFlashcardRequest request);
    Task<FlashcardDeckDto> CreateDeckAsync(int userId, CreateDeckRequest request);
    Task<FlashcardDto> AddCardAsync(int userId, CreateFlashcardRequest request);
    Task DeleteDeckAsync(int userId, int deckId);
    Task<int> GetTotalDueCountAsync(int userId);
}
