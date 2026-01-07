using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

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
