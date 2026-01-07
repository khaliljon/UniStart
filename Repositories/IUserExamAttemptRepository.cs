using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

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
