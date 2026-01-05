using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository implementation for UserExamAttempt entity
/// </summary>
public class UserExamAttemptRepository : Repository<UserExamAttempt>, IUserExamAttemptRepository
{
    public UserExamAttemptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserExamAttempt>> GetUserAttemptsAsync(string userId)
    {
        return await _dbSet
            .Where(ea => ea.UserId == userId && ea.CompletedAt != null)
            .Include(ea => ea.Exam)
            .OrderByDescending(ea => ea.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserExamAttempt>> GetExamAttemptsAsync(int examId)
    {
        return await _dbSet
            .Where(ea => ea.ExamId == examId && ea.CompletedAt != null)
            .Include(ea => ea.User)
            .OrderByDescending(ea => ea.CompletedAt)
            .ToListAsync();
    }

    public async Task<UserExamAttempt?> GetAttemptWithAnswersAsync(int attemptId)
    {
        return await _dbSet
            .Include(ea => ea.UserAnswers)
            .Include(ea => ea.Exam)
            .ThenInclude(e => e.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(ea => ea.Id == attemptId);
    }

    public async Task<int> GetAttemptCountForExamAsync(string userId, int examId)
    {
        return await _dbSet
            .Where(ea => ea.UserId == userId && ea.ExamId == examId)
            .CountAsync();
    }

    public async Task<IEnumerable<UserExamAttempt>> GetRecentAttemptsAsync(string userId, int count)
    {
        return await _dbSet
            .Where(ea => ea.UserId == userId && ea.CompletedAt != null)
            .Include(ea => ea.Exam)
            .OrderByDescending(ea => ea.CompletedAt)
            .Take(count)
            .ToListAsync();
    }
}
