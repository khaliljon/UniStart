using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository implementation for UserFlashcardProgress entity
/// </summary>
public class UserFlashcardProgressRepository : Repository<UserFlashcardProgress>, IUserFlashcardProgressRepository
{
    public UserFlashcardProgressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserFlashcardProgress>> GetUserProgressAsync(string userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId)
            .Include(p => p.Flashcard)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserFlashcardProgress>> GetProgressForSetAsync(string userId, int setId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId && p.Flashcard.FlashcardSetId == setId)
            .Include(p => p.Flashcard)
            .ToListAsync();
    }

    public async Task<UserFlashcardProgress?> GetProgressAsync(string userId, int flashcardId)
    {
        return await _dbSet
            .Include(p => p.Flashcard)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.FlashcardId == flashcardId);
    }

    public async Task<int> GetMasteredCardsCountAsync(string userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId && p.IsMastered)
            .CountAsync();
    }

    public async Task<IEnumerable<UserFlashcardProgress>> GetCardsDueForReviewAsync(string userId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(p => p.UserId == userId && p.NextReviewDate <= now)
            .Include(p => p.Flashcard)
            .OrderBy(p => p.NextReviewDate)
            .ToListAsync();
    }
}
