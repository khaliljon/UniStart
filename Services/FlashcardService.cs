using Microsoft.EntityFrameworkCore;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Repositories;

namespace UniStart.Services;

/// <summary>
/// Service implementation for Flashcard business logic
/// </summary>
public class FlashcardService : IFlashcardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISpacedRepetitionService _spacedRepetition;
    private readonly ILogger<FlashcardService> _logger;

    public FlashcardService(
        IUnitOfWork unitOfWork,
        ISpacedRepetitionService spacedRepetition,
        ILogger<FlashcardService> logger)
    {
        _unitOfWork = unitOfWork;
        _spacedRepetition = spacedRepetition;
        _logger = logger;
    }

    public async Task<FlashcardSet?> GetSetByIdAsync(int id)
    {
        return await _unitOfWork.FlashcardSets.GetByIdAsync(id);
    }

    public async Task<FlashcardSet?> GetSetWithCardsAsync(int id)
    {
        return await _unitOfWork.FlashcardSets.GetSetWithFlashcardsAsync(id);
    }

    public async Task<IEnumerable<FlashcardSet>> GetPublicSetsAsync()
    {
        return await _unitOfWork.FlashcardSets.GetPublicSetsAsync();
    }

    public async Task<IEnumerable<FlashcardSet>> GetUserSetsAsync(string userId)
    {
        return await _unitOfWork.FlashcardSets.GetSetsByUserAsync(userId);
    }

    public async Task<IEnumerable<FlashcardSet>> GetSetsBySubjectAsync(string subject)
    {
        return await _unitOfWork.FlashcardSets.GetSetsBySubjectAsync(subject);
    }

    public async Task<FlashcardSet> CreateSetAsync(string userId, CreateFlashcardSetDto dto)
    {
        var set = new FlashcardSet
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            IsPublic = dto.IsPublic,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.FlashcardSets.AddAsync(set);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Flashcard set created: {Title} by user {UserId}", set.Title, userId);

        return set;
    }

    public async Task<FlashcardSet> UpdateSetAsync(int id, UpdateFlashcardSetDto dto)
    {
        var set = await _unitOfWork.FlashcardSets.GetByIdAsync(id);
        if (set == null)
            throw new InvalidOperationException($"Flashcard set {id} not found");

        set.Title = dto.Title;
        set.Description = dto.Description;
        set.Subject = dto.Subject;
        set.IsPublic = dto.IsPublic;
        set.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.FlashcardSets.Update(set);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Flashcard set updated: {Id}", id);

        return set;
    }

    public async Task<bool> DeleteSetAsync(int id, string userId)
    {
        var set = await _unitOfWork.FlashcardSets.GetByIdAsync(id);
        if (set == null)
            return false;

        // Проверяем владельца
        if (set.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete set {SetId} owned by {OwnerId}", 
                userId, id, set.UserId);
            return false;
        }

        _unitOfWork.FlashcardSets.Remove(set);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Flashcard set deleted: {Id}", id);

        return true;
    }

    public async Task<Flashcard> AddCardToSetAsync(int setId, CreateFlashcardDto dto)
    {
        var set = await _unitOfWork.FlashcardSets.GetByIdAsync(setId);
        if (set == null)
            throw new InvalidOperationException($"Flashcard set {setId} not found");

        var card = new Flashcard
        {
            Type = dto.Type,
            Question = dto.Question,
            Answer = dto.Answer,
            OptionsJson = dto.OptionsJson,
            MatchingPairsJson = dto.MatchingPairsJson,
            SequenceJson = dto.SequenceJson,
            Explanation = dto.Explanation,
            FlashcardSetId = setId
        };

        await _unitOfWork.Repository<Flashcard>().AddAsync(card);
        
        set.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.FlashcardSets.Update(set);
        
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Flashcard added to set {SetId}", setId);

        return card;
    }

    public async Task<Flashcard> UpdateCardAsync(int cardId, UpdateFlashcardDto dto)
    {
        var card = await _unitOfWork.Repository<Flashcard>().GetByIdAsync(cardId);
        if (card == null)
            throw new InvalidOperationException($"Flashcard {cardId} not found");

        card.Type = dto.Type;
        card.Question = dto.Question;
        card.Answer = dto.Answer;
        card.OptionsJson = dto.OptionsJson;
        card.MatchingPairsJson = dto.MatchingPairsJson;
        card.SequenceJson = dto.SequenceJson;
        card.Explanation = dto.Explanation;

        _unitOfWork.Repository<Flashcard>().Update(card);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Flashcard updated: {Id}", cardId);

        return card;
    }

    public async Task<bool> DeleteCardAsync(int cardId)
    {
        var card = await _unitOfWork.Repository<Flashcard>().GetByIdAsync(cardId);
        if (card == null)
            return false;

        _unitOfWork.Repository<Flashcard>().Remove(card);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Flashcard deleted: {Id}", cardId);

        return true;
    }

    public async Task<IEnumerable<Flashcard>> GetCardsDueForReviewAsync(string userId, int setId)
    {
        var now = DateTime.UtcNow;
        
        var progressList = await _unitOfWork.FlashcardProgress.Query()
            .Where(p => p.UserId == userId && p.Flashcard.FlashcardSetId == setId && p.NextReviewDate <= now)
            .Include(p => p.Flashcard)
            .ToListAsync();

        return progressList.Select(p => p.Flashcard).ToList();
    }

    public async Task ReviewCardAsync(string userId, int cardId, int quality)
    {
        var progress = await _unitOfWork.FlashcardProgress.GetProgressAsync(userId, cardId);
        
        if (progress == null)
        {
            // Создаем новую запись прогресса
            progress = new UserFlashcardProgress
            {
                UserId = userId,
                FlashcardId = cardId,
                Repetitions = 0,
                EaseFactor = 2.5,
                Interval = 0,
                NextReviewDate = DateTime.UtcNow,
                LastReviewedAt = null
            };
            await _unitOfWork.FlashcardProgress.AddAsync(progress);
        }

        // Используем SpacedRepetitionService для вычисления следующего интервала
        _spacedRepetition.UpdateUserFlashcardProgress(progress, quality);
        
        _unitOfWork.FlashcardProgress.Update(progress);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Card {CardId} reviewed by user {UserId} with quality {Quality}", 
            cardId, userId, quality);
    }

    public async Task<UserFlashcardProgress?> GetCardProgressAsync(string userId, int cardId)
    {
        return await _unitOfWork.FlashcardProgress.GetProgressAsync(userId, cardId);
    }

    public async Task<Dictionary<string, object>> GetSetStatisticsAsync(string userId, int setId)
    {
        var set = await _unitOfWork.FlashcardSets.GetSetWithFlashcardsAsync(setId);
        if (set == null)
            return new Dictionary<string, object>();

        var totalCards = set.Flashcards.Count;
        
        var progressList = await _unitOfWork.FlashcardProgress.GetProgressForSetAsync(userId, setId);
        
        var reviewedCards = progressList.Count(p => p.LastReviewedAt != null);
        var masteredCards = progressList.Count(p => p.IsMastered);
        var cardsDueForReview = progressList.Count(p => p.NextReviewDate <= DateTime.UtcNow);

        var access = await _unitOfWork.Repository<UserFlashcardSetAccess>()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.FlashcardSetId == setId);

        return new Dictionary<string, object>
        {
            ["totalCards"] = totalCards,
            ["reviewedCards"] = reviewedCards,
            ["masteredCards"] = masteredCards,
            ["cardsDueForReview"] = cardsDueForReview,
            ["progressPercentage"] = totalCards > 0 ? (reviewedCards * 100.0 / totalCards) : 0,
            ["masteryPercentage"] = totalCards > 0 ? (masteredCards * 100.0 / totalCards) : 0,
            ["isCompleted"] = access?.IsCompleted ?? false,
            ["lastAccessedAt"] = access?.LastAccessedAt ?? access?.FirstAccessedAt
        };
    }
}
