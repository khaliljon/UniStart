using Microsoft.EntityFrameworkCore;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
using UniStart.Repositories;

namespace UniStart.Services;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExamService> _logger;

    public ExamService(IUnitOfWork unitOfWork, ILogger<ExamService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Exam?> GetExamByIdAsync(int id)
    {
        return await _unitOfWork.Exams.GetByIdAsync(id);
    }

    public async Task<Exam?> GetExamWithQuestionsAsync(int id)
    {
        return await _unitOfWork.Exams.GetExamWithQuestionsAsync(id);
    }

    public async Task<ExamDto?> GetExamDetailAsync(int id, string? userId, bool isAdmin)
    {
        var exam = await _unitOfWork.Repository<Exam>()
            .Query()
            .Include(t => t.User)
            .Include(t => t.Tags)
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (exam == null)
            return null;

        var isOwnerOrAdmin = exam.UserId == userId || isAdmin;

        // Проверка доступа: должен быть опубликован или создан текущим пользователем или админом
        if (!exam.IsPublished && !isOwnerOrAdmin)
            return null;

        var examDto = new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Subject = exam.Subject,
            Difficulty = exam.Difficulty,
            CountryId = exam.CountryId,
            UniversityId = exam.UniversityId,
            ExamTypeId = exam.ExamTypeId,
            MaxAttempts = exam.MaxAttempts,
            PassingScore = exam.PassingScore,
            IsProctored = exam.IsProctored,
            ShuffleQuestions = exam.ShuffleQuestions,
            ShuffleAnswers = exam.ShuffleAnswers,
            ShowResultsAfter = exam.ShowResultsAfter,
            ShowCorrectAnswers = exam.ShowCorrectAnswers,
            ShowDetailedFeedback = exam.ShowDetailedFeedback,
            TimeLimit = exam.TimeLimit,
            StrictTiming = exam.StrictTiming,
            IsPublished = exam.IsPublished,
            IsPublic = exam.IsPublic,
            TotalPoints = exam.TotalPoints,
            QuestionCount = exam.Questions.Count,
            CreatedAt = exam.CreatedAt,
            UserId = exam.UserId,
            UserName = exam.User.UserName ?? "Unknown",
            Tags = exam.Tags.Select(tag => tag.Name).ToList()
        };

        // Только владелец или админ видят вопросы с правильными ответами
        if (isOwnerOrAdmin)
        {
            examDto.Questions = exam.Questions.OrderBy(q => q.Order).Select(q => new ExamQuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Explanation = q.Explanation,
                QuestionType = q.QuestionType,
                Points = q.Points,
                Order = q.Order,
                Answers = q.Answers.OrderBy(a => a.Order).Select(a => new ExamAnswerDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect,
                    Order = a.Order
                }).ToList()
            }).ToList();
        }

        return examDto;
    }

    public async Task<IEnumerable<Exam>> GetPublishedExamsAsync()
    {
        return await _unitOfWork.Exams.GetPublishedExamsAsync();
    }

    public async Task<PagedResult<ExamDto>> SearchExamsAsync(ExamFilterDto filter, bool onlyPublished = true)
    {
        var query = _unitOfWork.Repository<Exam>().Query();

        if (onlyPublished)
            query = query.Where(e => e.IsPublished);

        // Поиск
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(searchLower) ||
                (e.Description != null && e.Description.ToLower().Contains(searchLower)));
        }

        // Фильтры
        if (!string.IsNullOrEmpty(filter.Subject))
            query = query.Where(e => e.Subject == filter.Subject);

        if (!string.IsNullOrEmpty(filter.Difficulty))
            query = query.Where(e => e.Difficulty == filter.Difficulty);

        // Подсчет общего количества
        var totalCount = await query.CountAsync();

        // Пагинация
        var exams = await query
            .AsNoTracking()
            .Include(e => e.Questions)
            .Include(e => e.User)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Subject = e.Subject,
                Difficulty = e.Difficulty,
                TimeLimit = e.TimeLimit,
                PassingScore = e.PassingScore,
                MaxAttempts = e.MaxAttempts,
                QuestionCount = e.Questions.Count,
                TotalPoints = e.Questions.Sum(q => q.Points),
                UserId = e.UserId,
                UserName = e.User.UserName ?? "",
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ExamDto>
        {
            Items = exams,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<ExamDto>> GetMyExamsAsync(string userId, ExamFilterDto filter)
    {
        var query = _unitOfWork.Repository<Exam>()
            .Query()
            .Where(e => e.UserId == userId);

        // Поиск
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(searchLower) ||
                (e.Description != null && e.Description.ToLower().Contains(searchLower)));
        }

        // Фильтры
        if (!string.IsNullOrEmpty(filter.Subject))
            query = query.Where(e => e.Subject == filter.Subject);

        if (!string.IsNullOrEmpty(filter.Difficulty))
            query = query.Where(e => e.Difficulty == filter.Difficulty);

        // Подсчет общего количества
        var totalCount = await query.CountAsync();

        // Пагинация
        var exams = await query
            .Include(e => e.Questions)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Subject = e.Subject,
                Difficulty = e.Difficulty,
                TimeLimit = e.TimeLimit,
                PassingScore = e.PassingScore,
                MaxAttempts = e.MaxAttempts,
                IsPublished = e.IsPublished,
                QuestionCount = e.Questions.Count,
                TotalPoints = e.Questions.Sum(q => q.Points),
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ExamDto>
        {
            Items = exams,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<Exam>> GetExamsByUserAsync(string userId)
    {
        return await _unitOfWork.Repository<Exam>()
            .FindAsync(e => e.UserId == userId);
    }

    public async Task<IEnumerable<Exam>> GetExamsBySubjectAsync(string subject)
    {
        return await _unitOfWork.Repository<Exam>()
            .FindAsync(e => e.Subject == subject && e.IsPublic);
    }

    public async Task<Exam> CreateExamAsync(string userId, CreateExamDto dto)
    {
        var exam = new Exam
        {
            Title = dto.Title,
            Description = dto.Description,
            Difficulty = dto.Difficulty,
            CountryId = dto.CountryId,
            UniversityId = dto.UniversityId,
            ExamTypeId = dto.ExamTypeId,
            TimeLimit = dto.TimeLimit,
            PassingScore = dto.PassingScore,
            MaxAttempts = dto.MaxAttempts,
            IsProctored = dto.IsProctored,
            ShuffleQuestions = dto.ShuffleQuestions,
            ShuffleAnswers = dto.ShuffleAnswers,
            ShowResultsAfter = dto.ShowResultsAfter,
            ShowCorrectAnswers = dto.ShowCorrectAnswers,
            ShowDetailedFeedback = dto.ShowDetailedFeedback,
            StrictTiming = dto.StrictTiming,
            UserId = userId,
            IsPublished = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Exam>().AddAsync(exam);
        await _unitOfWork.SaveChangesAsync();

        // Добавляем предметы после создания экзамена
        if (dto.SubjectIds.Any())
        {
            var subjects = await _unitOfWork.Repository<Subject>()
                .Query()
                .Where(s => dto.SubjectIds.Contains(s.Id))
                .ToListAsync();
            
            foreach (var subject in subjects)
            {
                exam.Subjects.Add(subject);
            }
        }

        // Добавляем теги
        if (dto.TagIds.Any())
        {
            var tags = await _unitOfWork.Repository<Tag>()
                .Query()
                .Where(t => dto.TagIds.Contains(t.Id))
                .ToListAsync();
            
            foreach (var tag in tags)
            {
                exam.Tags.Add(tag);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam created with ID {ExamId} by user {UserId}", exam.Id, userId);
        return exam;
    }

    public async Task<Exam> UpdateExamAsync(int id, string userId, UpdateExamDto dto)
    {
        var exam = await _unitOfWork.Repository<Exam>()
            .Query()
            .Include(e => e.Subjects)
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
            throw new InvalidOperationException($"Экзамен с ID {id} не найден");

        if (exam.UserId != userId)
            throw new UnauthorizedAccessException("Только владелец экзамена может его редактировать");

        // Обновляем основные поля
        exam.Title = dto.Title;
        exam.Description = dto.Description;
        exam.Difficulty = dto.Difficulty;
        exam.CountryId = dto.CountryId;
        exam.UniversityId = dto.UniversityId;
        exam.ExamTypeId = dto.ExamTypeId;
        exam.MaxAttempts = dto.MaxAttempts;
        exam.PassingScore = dto.PassingScore;
        exam.IsProctored = dto.IsProctored;
        exam.ShuffleQuestions = dto.ShuffleQuestions;
        exam.ShuffleAnswers = dto.ShuffleAnswers;
        exam.ShowResultsAfter = dto.ShowResultsAfter;
        exam.ShowCorrectAnswers = dto.ShowCorrectAnswers;
        exam.ShowDetailedFeedback = dto.ShowDetailedFeedback;
        exam.TimeLimit = dto.TimeLimit;
        exam.StrictTiming = dto.StrictTiming;
        exam.IsPublished = dto.IsPublished;
        exam.IsPublic = dto.IsPublic;
        exam.UpdatedAt = DateTime.UtcNow;

        // Обновляем предметы (many-to-many)
        exam.Subjects.Clear();
        if (dto.SubjectIds.Any())
        {
            var subjects = await _unitOfWork.Repository<Subject>()
                .Query()
                .Where(s => dto.SubjectIds.Contains(s.Id))
                .ToListAsync();
            
            foreach (var subject in subjects)
            {
                exam.Subjects.Add(subject);
            }
        }

        // Обновляем теги
        if (dto.TagIds.Any())
        {
            exam.Tags.Clear();
            var tags = await _unitOfWork.Repository<Tag>()
                .Query()
                .Where(t => dto.TagIds.Contains(t.Id))
                .ToListAsync();
            
            foreach (var tag in tags)
            {
                exam.Tags.Add(tag);
            }
        }

        _unitOfWork.Repository<Exam>().Update(exam);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam {ExamId} updated by user {UserId}", id, userId);
        return exam;
    }

    public async Task<bool> DeleteExamAsync(int id, string userId)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(id);
        if (exam == null || exam.UserId != userId)
            return false;

        _unitOfWork.Repository<Exam>().Remove(exam);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam {ExamId} deleted by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> PublishExamAsync(int id, string userId)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(id);
        if (exam == null || exam.UserId != userId)
            return false;

        exam.IsPublished = true;
        exam.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Exam>().Update(exam);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam {ExamId} published by user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> CanUserAccessExamAsync(int examId, string userId)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
        if (exam == null)
            return false;

        // Владелец или публичный экзамен
        return exam.UserId == userId || exam.IsPublic;
    }

    public async Task<UserExamAttempt> StartExamAttemptAsync(int examId, string userId)
    {
        var exam = await _unitOfWork.Exams.GetExamWithQuestionsAsync(examId);
        if (exam == null)
            throw new KeyNotFoundException($"Exam with ID {examId} not found");

        if (!await CanUserAccessExamAsync(examId, userId))
            throw new UnauthorizedAccessException("User does not have access to this exam");

        // Проверка лимита попыток
        var existingAttempts = await _unitOfWork.ExamAttempts.GetUserAttemptsAsync(userId);
        var attemptCount = existingAttempts.Count(a => a.ExamId == examId);
        
        if (attemptCount >= exam.MaxAttempts)
            throw new InvalidOperationException($"Maximum attempts ({exam.MaxAttempts}) reached");

        var attempt = new UserExamAttempt
        {
            UserId = userId,
            ExamId = examId,
            StartedAt = DateTime.UtcNow,
            AttemptNumber = attemptCount + 1,
            Score = 0,
            TotalPoints = exam.Questions.Sum(q => q.Points)
        };

        await _unitOfWork.ExamAttempts.AddAsync(attempt);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam attempt {AttemptId} (#{AttemptNumber}) started for exam {ExamId} by user {UserId}", 
            attempt.Id, attempt.AttemptNumber, examId, userId);
        return attempt;
    }

    public async Task<UserExamAttempt> SubmitExamAttemptAsync(int attemptId, SubmitExamDto dto)
    {
        var attempt = await _unitOfWork.ExamAttempts.GetAttemptWithAnswersAsync(attemptId);
        if (attempt == null)
            throw new KeyNotFoundException($"Attempt with ID {attemptId} not found");

        if (attempt.CompletedAt != null)
            throw new InvalidOperationException("Attempt already completed");

        var exam = await _unitOfWork.Exams.GetExamWithQuestionsAsync(attempt.ExamId);
        if (exam == null)
            throw new KeyNotFoundException($"Exam with ID {attempt.ExamId} not found");

        // Подсчет баллов
        int totalScore = 0;
        int maxScore = 0;

        foreach (var question in exam.Questions)
        {
            maxScore += question.Points;
            
            var submittedAnswer = dto.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
            if (submittedAnswer != null)
            {
                // Для вопросов с одним правильным ответом
                if (question.QuestionType == "SingleChoice" || question.QuestionType == "TrueFalse")
                {
                    var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                    if (correctAnswer != null && submittedAnswer.SelectedAnswerId == correctAnswer.Id)
                    {
                        totalScore += question.Points;
                    }
                }
                // Для вопросов с несколькими правильными ответами (будущее расширение)
                else if (question.QuestionType == "MultipleChoice")
                {
                    // Здесь нужно использовать ExamAttemptAnswerDto с AnswerIds
                    // Пока пропускаем, т.к. SubmitExamDto использует SelectedAnswerId
                }
            }
        }

        attempt.CompletedAt = DateTime.UtcNow;
        attempt.Score = totalScore;
        attempt.TotalPoints = maxScore;
        attempt.Percentage = maxScore > 0 
            ? (totalScore / (double)maxScore) * 100 
            : 0;
        attempt.Passed = attempt.Percentage >= exam.PassingScore;
        attempt.TimeSpent = dto.TimeSpent;

        _unitOfWork.ExamAttempts.Update(attempt);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam attempt {AttemptId} completed with score {Score}/{TotalPoints}, passed: {Passed}", 
            attemptId, totalScore, maxScore, attempt.Passed);
        return attempt;
    }

    public async Task<IEnumerable<UserExamAttempt>> GetUserAttemptsAsync(string userId)
    {
        return await _unitOfWork.ExamAttempts.GetUserAttemptsAsync(userId);
    }

    public async Task<UserExamAttempt?> GetAttemptDetailsAsync(int attemptId)
    {
        return await _unitOfWork.ExamAttempts.GetAttemptWithAnswersAsync(attemptId);
    }
}
