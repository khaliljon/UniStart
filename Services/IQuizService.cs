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
/// Service for Quiz business logic
/// </summary>
public interface IQuizService
{
    Task<Quiz?> GetQuizByIdAsync(int id);
    Task<Quiz?> GetQuizWithQuestionsAsync(int id);
    Task<QuizDetailDto?> GetQuizDetailAsync(int id, string? userId, bool isAdmin);
    Task<IEnumerable<Quiz>> GetPublishedQuizzesAsync();
    Task<PagedResult<QuizDto>> SearchQuizzesAsync(QuizFilterDto filter, bool onlyPublished = true);
    Task<PagedResult<QuizDto>> GetMyQuizzesAsync(string userId, QuizFilterDto filter);
    Task<IEnumerable<Quiz>> GetQuizzesByUserAsync(string userId);
    Task<IEnumerable<Quiz>> GetQuizzesBySubjectAsync(string subject);
    Task<Quiz> CreateQuizAsync(string userId, CreateQuizDto dto);
    Task<Quiz> UpdateQuizAsync(int id, UpdateQuizDto dto);
    Task<bool> DeleteQuizAsync(int id, string? userId, bool isAdmin);
    Task<bool> PublishQuizAsync(int id, string userId);
    Task<bool> UnpublishQuizAsync(int id, string userId, bool isAdmin);
    Task<bool> CanUserAccessQuizAsync(int quizId, string userId);
    Task<UserQuizAttempt> StartQuizAttemptAsync(int quizId, string userId);
    Task<UserQuizAttempt> SubmitQuizAttemptAsync(int attemptId, SubmitQuizDto dto);
    Task<IEnumerable<UserQuizAttempt>> GetUserAttemptsAsync(string userId);
    Task<UserQuizAttempt?> GetAttemptDetailsAsync(int attemptId);
}
