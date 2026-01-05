using Microsoft.EntityFrameworkCore;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Repositories;

namespace UniStart.Services;

public class QuizService : IQuizService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuizService> _logger;

    public QuizService(IUnitOfWork unitOfWork, ILogger<QuizService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Quiz?> GetQuizByIdAsync(int id)
    {
        return await _unitOfWork.Quizzes.GetByIdAsync(id);
    }

    public async Task<Quiz?> GetQuizWithQuestionsAsync(int id)
    {
        return await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(id);
    }

    public async Task<QuizDetailDto?> GetQuizDetailAsync(int id, string? userId, bool isAdmin)
    {
        var quiz = await _unitOfWork.Repository<Quiz>()
            .Query()
            .Include(q => q.Questions)
                .ThenInclude(qu => qu.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
            return null;

        var isOwnerOrAdmin = quiz.UserId == userId || isAdmin;

        return new QuizDetailDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimit = quiz.TimeLimit,
            Subject = quiz.Subject,
            Difficulty = quiz.Difficulty,
            IsPublic = quiz.IsPublic,
            IsPublished = quiz.IsPublished,
            IsLearningMode = quiz.IsLearningMode,
            Questions = quiz.Questions.OrderBy(q => q.OrderIndex).Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Points = q.Points,
                ImageUrl = q.ImageUrl,
                Explanation = q.Explanation,
                Answers = q.Answers.OrderBy(a => a.OrderIndex).Select(a => new QuizAnswerDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = quiz.IsLearningMode || isOwnerOrAdmin ? a.IsCorrect : null
                }).ToList()
            }).ToList()
        };
    }

    public async Task<IEnumerable<Quiz>> GetPublishedQuizzesAsync()
    {
        return await _unitOfWork.Quizzes.GetPublishedQuizzesAsync();
    }

    public async Task<PagedResult<QuizDto>> SearchQuizzesAsync(QuizFilterDto filter, bool onlyPublished = true)
    {
        var query = _unitOfWork.Repository<Quiz>().Query();

        if (onlyPublished)
            query = query.Where(q => q.IsPublished);

        // Поиск
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(q =>
                q.Title.ToLower().Contains(searchLower) ||
                q.Description.ToLower().Contains(searchLower));
        }

        // Фильтры
        if (!string.IsNullOrEmpty(filter.Subject))
            query = query.Where(q => q.Subject == filter.Subject);

        if (!string.IsNullOrEmpty(filter.Difficulty))
            query = query.Where(q => q.Difficulty == filter.Difficulty);

        // Подсчет общего количества
        var totalCount = await query.CountAsync();

        // Пагинация
        var quizzes = await query
            .Include(q => q.Questions)
            .OrderByDescending(q => q.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                TimeLimit = q.TimeLimit,
                Subject = q.Subject,
                Difficulty = q.Difficulty,
                QuestionCount = q.Questions.Count,
                TotalPoints = q.Questions.Sum(qu => qu.Points)
            })
            .ToListAsync();

        return new PagedResult<QuizDto>
        {
            Items = quizzes,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<QuizDto>> GetMyQuizzesAsync(string userId, QuizFilterDto filter)
    {
        var query = _unitOfWork.Repository<Quiz>()
            .Query()
            .Where(q => q.UserId == userId);

        // Поиск
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(q =>
                q.Title.ToLower().Contains(searchLower) ||
                q.Description.ToLower().Contains(searchLower));
        }

        // Фильтры
        if (!string.IsNullOrEmpty(filter.Subject))
            query = query.Where(q => q.Subject == filter.Subject);

        if (!string.IsNullOrEmpty(filter.Difficulty))
            query = query.Where(q => q.Difficulty == filter.Difficulty);

        // Подсчет общего количества
        var totalCount = await query.CountAsync();

        // Пагинация
        var quizzes = await query
            .Include(q => q.Questions)
            .OrderByDescending(q => q.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                TimeLimit = q.TimeLimit,
                Subject = q.Subject,
                Difficulty = q.Difficulty,
                IsPublished = q.IsPublished,
                QuestionCount = q.Questions.Count,
                TotalPoints = q.Questions.Sum(qu => qu.Points)
            })
            .ToListAsync();

        return new PagedResult<QuizDto>
        {
            Items = quizzes,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByUserAsync(string userId)
    {
        return await _unitOfWork.Repository<Quiz>()
            .FindAsync(q => q.UserId == userId);
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesBySubjectAsync(string subject)
    {
        return await _unitOfWork.Repository<Quiz>()
            .FindAsync(q => q.Subject == subject && q.IsPublic);
    }

    public async Task<Quiz> CreateQuizAsync(string userId, CreateQuizDto dto)
    {
        var quiz = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            Difficulty = dto.Difficulty,
            TimeLimit = dto.TimeLimit,
            IsPublished = false,
            IsPublic = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Quiz>().AddAsync(quiz);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Quiz created with ID {QuizId} by user {UserId}", quiz.Id, userId);
        return quiz;
    }

    public async Task<Quiz> UpdateQuizAsync(int id, UpdateQuizDto dto)
    {
        var quiz = await _unitOfWork.Quizzes.GetByIdAsync(id);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz with ID {id} not found");

        quiz.Title = dto.Title;
        quiz.Description = dto.Description;
        quiz.Subject = dto.Subject;
        quiz.Difficulty = dto.Difficulty;
        quiz.TimeLimit = dto.TimeLimit;
        quiz.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Quiz>().Update(quiz);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Quiz {QuizId} updated", id);
        return quiz;
    }

    public async Task<bool> DeleteQuizAsync(int id, string? userId, bool isAdmin)
    {
        var quiz = await _unitOfWork.Quizzes.GetByIdAsync(id);
        if (quiz == null)
            return false;

        // Проверяем права: админ или владелец
        if (!isAdmin && quiz.UserId != userId)
            return false;

        _unitOfWork.Repository<Quiz>().Remove(quiz);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Quiz {QuizId} deleted by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> PublishQuizAsync(int id, string userId)
    {
        var quiz = await _unitOfWork.Quizzes.GetByIdAsync(id);
        if (quiz == null || quiz.UserId != userId)
            return false;

        quiz.IsPublished = true;
        quiz.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Quiz>().Update(quiz);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Quiz {QuizId} published by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> UnpublishQuizAsync(int id, string userId, bool isAdmin)
    {
        var quiz = await _unitOfWork.Quizzes.GetByIdAsync(id);
        if (quiz == null)
            return false;

        // Проверяем права: админ или владелец
        if (!isAdmin && quiz.UserId != userId)
            return false;

        quiz.IsPublished = false;
        quiz.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Quiz>().Update(quiz);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Quiz {QuizId} unpublished by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> CanUserAccessQuizAsync(int quizId, string userId)
    {
        var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
        if (quiz == null)
            return false;

        // Владелец или публичный квиз
        return quiz.UserId == userId || quiz.IsPublic;
    }

    public async Task<UserQuizAttempt> StartQuizAttemptAsync(int quizId, string userId)
    {
        var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz with ID {quizId} not found");

        if (!await CanUserAccessQuizAsync(quizId, userId))
            throw new UnauthorizedAccessException("User does not have access to this quiz");

        var attempt = new UserQuizAttempt
        {
            UserId = userId,
            QuizId = quizId,
            StartedAt = DateTime.UtcNow,
            Score = 0,
            MaxScore = quiz.Questions.Sum(q => q.Points)
        };

        await _unitOfWork.QuizAttempts.AddAsync(attempt);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Quiz attempt {AttemptId} started for quiz {QuizId} by user {UserId}", 
            attempt.Id, quizId, userId);
        return attempt;
    }

    public async Task<UserQuizAttempt> SubmitQuizAttemptAsync(int attemptId, SubmitQuizDto dto)
    {
        var attempt = await _unitOfWork.QuizAttempts.GetAttemptWithAnswersAsync(attemptId);
        if (attempt == null)
            throw new KeyNotFoundException($"Attempt with ID {attemptId} not found");

        if (attempt.CompletedAt != null)
            throw new InvalidOperationException("Attempt already completed");

        var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(attempt.QuizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz with ID {attempt.QuizId} not found");

        // Подсчет баллов
        int totalScore = 0;
        int maxScore = 0;

        foreach (var question in quiz.Questions)
        {
            maxScore += question.Points;
            
            if (dto.UserAnswers.TryGetValue(question.Id, out var selectedAnswerIds))
            {
                var correctAnswerIds = question.Answers
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToHashSet();

                // Проверка правильности ответа
                if (correctAnswerIds.SetEquals(selectedAnswerIds))
                {
                    totalScore += question.Points;
                }
            }
        }

        attempt.CompletedAt = DateTime.UtcNow;
        attempt.Score = totalScore;
        attempt.MaxScore = maxScore;
        attempt.Percentage = maxScore > 0 ? (totalScore / (double)maxScore) * 100 : 0;
        attempt.TimeSpentSeconds = dto.TimeSpentSeconds;

        _unitOfWork.QuizAttempts.Update(attempt);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Quiz attempt {AttemptId} completed with score {Score}/{MaxScore}", 
            attemptId, totalScore, maxScore);
        return attempt;
    }

    public async Task<IEnumerable<UserQuizAttempt>> GetUserAttemptsAsync(string userId)
    {
        return await _unitOfWork.QuizAttempts.GetUserAttemptsAsync(userId);
    }

    public async Task<UserQuizAttempt?> GetAttemptDetailsAsync(int attemptId)
    {
        return await _unitOfWork.QuizAttempts.GetAttemptWithAnswersAsync(attemptId);
    }
}
