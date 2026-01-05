using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository for UserExamAttempt entity with specific operations
/// </summary>
public interface IUserExamAttemptRepository : IRepository<UserExamAttempt>
{
    Task<IEnumerable<UserExamAttempt>> GetUserAttemptsAsync(string userId);
    Task<IEnumerable<UserExamAttempt>> GetExamAttemptsAsync(int examId);
    Task<UserExamAttempt?> GetAttemptWithAnswersAsync(int attemptId);
    Task<int> GetAttemptCountForExamAsync(string userId, int examId);
    Task<IEnumerable<UserExamAttempt>> GetRecentAttemptsAsync(string userId, int count);
}
