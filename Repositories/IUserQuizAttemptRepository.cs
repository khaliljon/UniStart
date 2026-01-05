using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository for UserQuizAttempt entity with specific operations
/// </summary>
public interface IUserQuizAttemptRepository : IRepository<UserQuizAttempt>
{
    Task<IEnumerable<UserQuizAttempt>> GetUserAttemptsAsync(string userId);
    Task<IEnumerable<UserQuizAttempt>> GetQuizAttemptsAsync(int quizId);
    Task<UserQuizAttempt?> GetAttemptWithAnswersAsync(int attemptId);
    Task<int> GetAttemptCountForQuizAsync(string userId, int quizId);
    Task<IEnumerable<UserQuizAttempt>> GetRecentAttemptsAsync(string userId, int count);
}
