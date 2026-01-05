using UniStart.Models;

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
