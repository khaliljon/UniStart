using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository implementation for Quiz entity
/// </summary>
public class QuizRepository : Repository<Quiz>, IQuizRepository
{
    public QuizRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Quiz>> GetPublishedQuizzesAsync()
    {
        return await _dbSet
            .Where(q => q.IsPublished)
            .Include(q => q.User)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesBySubjectAsync(string subject)
    {
        return await _dbSet
            .Where(q => q.Subject == subject)
            .Include(q => q.User)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByUserAsync(string userId)
    {
        return await _dbSet
            .Where(q => q.UserId == userId)
            .Include(q => q.Questions)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
    {
        return await _dbSet
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .Include(q => q.User)
            .FirstOrDefaultAsync(q => q.Id == quizId);
    }

    public async Task<Quiz?> GetQuizWithAttemptsAsync(int quizId)
    {
        return await _dbSet
            .Include(q => q.Attempts)
            .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == quizId);
    }
}
