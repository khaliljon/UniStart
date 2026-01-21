using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Services;

/// <summary>
/// Service for Exam business logic
/// </summary>
public interface IExamService
{
    Task<Exam?> GetExamByIdAsync(int id);
    Task<Exam?> GetExamWithQuestionsAsync(int id);
    Task<ExamDto?> GetExamDetailAsync(int id, string? userId, bool isAdmin);
    Task<IEnumerable<Exam>> GetPublishedExamsAsync();
    Task<PagedResult<ExamDto>> SearchExamsAsync(ExamFilterDto filter, string? userId = null, bool onlyPublished = true, bool isAdmin = false, bool isTeacher = false);
    Task<PagedResult<ExamDto>> GetMyExamsAsync(string userId, ExamFilterDto filter);
    Task<IEnumerable<Exam>> GetExamsByUserAsync(string userId);
    Task<IEnumerable<Exam>> GetExamsBySubjectAsync(string subject);
    Task<Exam> CreateExamAsync(string userId, CreateExamDto dto);
    Task<Exam> UpdateExamAsync(int id, string userId, UpdateExamDto dto, bool isAdmin = false);
    Task<bool> DeleteExamAsync(int id, string userId, bool isAdmin = false);
    Task<bool> PublishExamAsync(int id, string userId, bool isAdmin = false);
    Task<bool> CanUserAccessExamAsync(int examId, string userId);
    Task<UserExamAttempt> StartExamAttemptAsync(int examId, string userId);
    Task<UserExamAttempt> SubmitExamAttemptAsync(int attemptId, SubmitExamDto dto);
    Task<IEnumerable<UserExamAttempt>> GetUserAttemptsAsync(string userId);
    Task<UserExamAttempt?> GetAttemptDetailsAsync(int attemptId);
}
