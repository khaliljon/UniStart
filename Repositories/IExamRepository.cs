using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Repositories;

/// <summary>
/// Repository for Exam entity with specific operations
/// </summary>
public interface IExamRepository : IRepository<Exam>
{
    Task<IEnumerable<Exam>> GetPublishedExamsAsync();
    Task<IEnumerable<Exam>> GetExamsBySubjectAsync(string subject);
    Task<IEnumerable<Exam>> GetExamsByUserAsync(string userId);
    Task<Exam?> GetExamWithQuestionsAsync(int examId);
    Task<Exam?> GetExamWithAttemptsAsync(int examId);
}
