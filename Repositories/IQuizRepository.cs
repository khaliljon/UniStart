using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository for Quiz entity with specific operations
/// </summary>
public interface IQuizRepository : IRepository<Quiz>
{
    Task<IEnumerable<Quiz>> GetPublishedQuizzesAsync();
    Task<IEnumerable<Quiz>> GetQuizzesBySubjectAsync(string subject);
    Task<IEnumerable<Quiz>> GetQuizzesByUserAsync(string userId);
    Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
    Task<Quiz?> GetQuizWithAttemptsAsync(int quizId);
}
