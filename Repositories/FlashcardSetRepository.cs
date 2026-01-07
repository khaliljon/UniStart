using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Repositories;

/// <summary>
/// Repository implementation for FlashcardSet entity
/// </summary>
public class FlashcardSetRepository : Repository<FlashcardSet>, IFlashcardSetRepository
{
    public FlashcardSetRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FlashcardSet>> GetPublicSetsAsync()
    {
        return await _dbSet
            .Where(fs => fs.IsPublic)
            .Include(fs => fs.User)
            .OrderByDescending(fs => fs.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FlashcardSet>> GetSetsByUserAsync(string userId)
    {
        return await _dbSet
            .Where(fs => fs.UserId == userId)
            .Include(fs => fs.Flashcards)
            .OrderByDescending(fs => fs.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FlashcardSet>> GetSetsBySubjectAsync(string subject)
    {
        return await _dbSet
            .Where(fs => fs.Subject == subject && fs.IsPublic)
            .Include(fs => fs.User)
            .OrderByDescending(fs => fs.CreatedAt)
            .ToListAsync();
    }

    public async Task<FlashcardSet?> GetSetWithFlashcardsAsync(int setId)
    {
        return await _dbSet
            .Include(fs => fs.Flashcards)
            .Include(fs => fs.User)
            .FirstOrDefaultAsync(fs => fs.Id == setId);
    }

    public async Task<FlashcardSet?> GetSetWithProgressAsync(int setId, string userId)
    {
        return await _dbSet
            .Include(fs => fs.Flashcards)
            .FirstOrDefaultAsync(fs => fs.Id == setId);
    }
}
