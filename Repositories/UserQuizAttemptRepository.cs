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
/// Repository implementation for UserQuizAttempt entity
/// </summary>
public class UserQuizAttemptRepository : Repository<UserQuizAttempt>, IUserQuizAttemptRepository
{
    public UserQuizAttemptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserQuizAttempt>> GetUserAttemptsAsync(string userId)
    {
        return await _dbSet
            .Where(qa => qa.UserId == userId && qa.CompletedAt != null)
            .Include(qa => qa.Quiz)
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserQuizAttempt>> GetQuizAttemptsAsync(int quizId)
    {
        return await _dbSet
            .Where(qa => qa.QuizId == quizId && qa.CompletedAt != null)
            .Include(qa => qa.User)
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();
    }

    public async Task<UserQuizAttempt?> GetAttemptWithAnswersAsync(int attemptId)
    {
        return await _dbSet
            .Include(qa => qa.Quiz)
            .ThenInclude(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(qa => qa.Id == attemptId);
    }

    public async Task<int> GetAttemptCountForQuizAsync(string userId, int quizId)
    {
        return await _dbSet
            .Where(qa => qa.UserId == userId && qa.QuizId == quizId)
            .CountAsync();
    }

    public async Task<IEnumerable<UserQuizAttempt>> GetRecentAttemptsAsync(string userId, int count)
    {
        return await _dbSet
            .Where(qa => qa.UserId == userId && qa.CompletedAt != null)
            .Include(qa => qa.Quiz)
            .OrderByDescending(qa => qa.CompletedAt)
            .Take(count)
            .ToListAsync();
    }
}
