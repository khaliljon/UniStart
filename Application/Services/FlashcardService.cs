using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class FlashcardService : IFlashcardService
{
    private readonly UniStartDbContext _context;

    public FlashcardService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FlashcardDeckDto>> GetDecksAsync(int userId, string[]? examTypeCodes = null)
    {
        var query = _context.FlashcardDecks
            .Include(d => d.Cards)
            .Where(d => d.IsSystem || d.CreatedByUserId == userId);

        if (examTypeCodes != null && examTypeCodes.Length > 0)
            query = query.Where(d => d.ExamTypeCode == null || examTypeCodes.Contains(d.ExamTypeCode));

        var decks = await query.OrderByDescending(d => d.IsSystem).ThenBy(d => d.Title).ToListAsync();

        var deckIds = decks.SelectMany(d => d.Cards.Select(c => c.Id)).ToList();
        var progressMap = await _context.UserFlashcardProgress
            .Where(p => p.UserId == userId && deckIds.Contains(p.FlashcardId))
            .ToDictionaryAsync(p => p.FlashcardId);

        var now = DateTime.UtcNow;

        return decks.Select(d =>
        {
            var cardIds = d.Cards.Select(c => c.Id).ToList();
            var due = cardIds.Count(id =>
                !progressMap.ContainsKey(id) || progressMap[id].NextReviewAt <= now);
            var mastered = cardIds.Count(id =>
                progressMap.ContainsKey(id) && progressMap[id].IntervalDays >= 21);

            return new FlashcardDeckDto(
                d.Id, d.Title, d.Description, d.ExamTypeCode, d.TopicId,
                d.IsSystem, d.Cards.Count, due, mastered
            );
        });
    }

    public async Task<IEnumerable<FlashcardReviewDto>> GetDueCardsAsync(int userId, int deckId, int limit = 20)
    {
        var cards = await _context.Flashcards
            .Where(c => c.DeckId == deckId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var cardIds = cards.Select(c => c.Id).ToList();
        var progressMap = await _context.UserFlashcardProgress
            .Where(p => p.UserId == userId && cardIds.Contains(p.FlashcardId))
            .ToDictionaryAsync(p => p.FlashcardId);

        var now = DateTime.UtcNow;

        var dueCards = cards
            .Where(c => !progressMap.ContainsKey(c.Id) || progressMap[c.Id].NextReviewAt <= now)
            .Take(limit)
            .Select(c =>
            {
                progressMap.TryGetValue(c.Id, out var prog);
                return new FlashcardReviewDto(
                    c.Id, c.Front, c.Back,
                    prog?.IntervalDays,
                    prog?.EaseFactor
                );
            })
            .ToList();

        return dueCards;
    }

    /// <summary>
    /// SM-2 algorithm: update flashcard progress based on quality rating (0-5).
    /// </summary>
    public async Task ReviewCardAsync(int userId, ReviewFlashcardRequest request)
    {
        var progress = await _context.UserFlashcardProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.FlashcardId == request.FlashcardId);

        if (progress == null)
        {
            progress = new UserFlashcardProgress
            {
                UserId = userId,
                FlashcardId = request.FlashcardId,
                EaseFactor = 2.5,
                IntervalDays = 1,
                Repetitions = 0
            };
            _context.UserFlashcardProgress.Add(progress);
        }

        var quality = Math.Clamp(request.Quality, 0, 5);

        if (quality >= 3)
        {
            // Correct response
            progress.Repetitions++;
            if (progress.Repetitions == 1)
                progress.IntervalDays = 1;
            else if (progress.Repetitions == 2)
                progress.IntervalDays = 6;
            else
                progress.IntervalDays = (int)Math.Round(progress.IntervalDays * progress.EaseFactor);

            progress.EaseFactor = Math.Max(1.3,
                progress.EaseFactor + 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
        }
        else
        {
            // Incorrect — reset
            progress.Repetitions = 0;
            progress.IntervalDays = 1;
        }

        progress.LastQuality = quality;
        progress.LastReviewedAt = DateTime.UtcNow;
        progress.NextReviewAt = DateTime.UtcNow.AddDays(progress.IntervalDays);

        await _context.SaveChangesAsync();
    }

    public async Task<FlashcardDeckDto> CreateDeckAsync(int userId, CreateDeckRequest request)
    {
        var deck = new FlashcardDeck
        {
            Title = request.Title,
            Description = request.Description,
            ExamTypeCode = request.ExamTypeCode,
            TopicId = request.TopicId,
            IsSystem = false,
            CreatedByUserId = userId
        };

        _context.FlashcardDecks.Add(deck);
        await _context.SaveChangesAsync();

        return new FlashcardDeckDto(deck.Id, deck.Title, deck.Description,
            deck.ExamTypeCode, deck.TopicId, false, 0, 0, 0);
    }

    public async Task<FlashcardDto> AddCardAsync(int userId, CreateFlashcardRequest request)
    {
        var deck = await _context.FlashcardDecks.FindAsync(request.DeckId)
            ?? throw new ArgumentException("Deck not found");

        if (deck.IsSystem && deck.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("Cannot modify system decks");

        var maxSort = await _context.Flashcards
            .Where(c => c.DeckId == request.DeckId)
            .MaxAsync(c => (int?)c.SortOrder) ?? 0;

        var card = new Flashcard
        {
            DeckId = request.DeckId,
            Front = request.Front,
            Back = request.Back,
            SortOrder = maxSort + 1
        };

        _context.Flashcards.Add(card);
        await _context.SaveChangesAsync();

        return new FlashcardDto(card.Id, card.DeckId, card.Front, card.Back, card.SortOrder);
    }

    public async Task DeleteDeckAsync(int userId, int deckId)
    {
        var deck = await _context.FlashcardDecks.FindAsync(deckId)
            ?? throw new ArgumentException("Deck not found");

        if (deck.IsSystem || deck.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("Cannot delete this deck");

        _context.FlashcardDecks.Remove(deck);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetTotalDueCountAsync(int userId)
    {
        var now = DateTime.UtcNow;

        // Cards with progress due for review
        var dueWithProgress = await _context.UserFlashcardProgress
            .CountAsync(p => p.UserId == userId && p.NextReviewAt <= now);

        // New cards (no progress yet) from accessible decks
        var accessibleDeckIds = await _context.FlashcardDecks
            .Where(d => d.IsSystem || d.CreatedByUserId == userId)
            .Select(d => d.Id)
            .ToListAsync();

        var cardsWithProgress = await _context.UserFlashcardProgress
            .Where(p => p.UserId == userId)
            .Select(p => p.FlashcardId)
            .ToListAsync();

        var newCards = await _context.Flashcards
            .CountAsync(c => accessibleDeckIds.Contains(c.DeckId) && !cardsWithProgress.Contains(c.Id));

        return dueWithProgress + newCards;
    }
}
