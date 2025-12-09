using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExamsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<string> GetUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? throw new UnauthorizedAccessException("Пользователь не авторизован");
    }

    /// <summary>
    /// Получить все опубликованные экзамены (для студентов)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ExamDto>>> GetAllExams(
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Exams
            .Include(t => t.User)
            .Include(t => t.Tags)
            .Include(t => t.Questions)
            .Where(t => t.IsPublished);

        if (!string.IsNullOrEmpty(subject))
            query = query.Where(t => t.Subject == subject);

        if (!string.IsNullOrEmpty(difficulty))
            query = query.Where(t => t.Difficulty == difficulty);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.Title.Contains(search) || 
                                    (t.Description != null && t.Description.Contains(search)));

        var exams = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ExamDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Subject = t.Subject,
                Difficulty = t.Difficulty,
                MaxAttempts = t.MaxAttempts,
                PassingScore = t.PassingScore,
                IsProctored = t.IsProctored,
                ShuffleQuestions = t.ShuffleQuestions,
                ShuffleAnswers = t.ShuffleAnswers,
                ShowResultsAfter = t.ShowResultsAfter,
                ShowCorrectAnswers = t.ShowCorrectAnswers,
                ShowDetailedFeedback = t.ShowDetailedFeedback,
                TimeLimit = t.TimeLimit,
                IsPublished = t.IsPublished,
                IsPublic = t.IsPublic,
                TotalPoints = t.TotalPoints,
                QuestionCount = t.Questions.Count,
                CreatedAt = t.CreatedAt,
                UserId = t.UserId,
                UserName = t.User.UserName ?? "Unknown",
                Tags = t.Tags.Select(tag => tag.Name).ToList()
            })
            .ToListAsync();

        return Ok(exams);
    }

    /// <summary>
    /// Получить свои экзамены (для преподавателей и админов)
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<List<ExamDto>>> GetMyExams()
    {
        var userId = await GetUserId();

        var exams = await _context.Exams
            .Include(t => t.User)
            .Include(t => t.Tags)
            .Include(t => t.Questions)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ExamDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Subject = t.Subject,
                Difficulty = t.Difficulty,
                MaxAttempts = t.MaxAttempts,
                PassingScore = t.PassingScore,
                IsProctored = t.IsProctored,
                ShuffleQuestions = t.ShuffleQuestions,
                ShuffleAnswers = t.ShuffleAnswers,
                ShowResultsAfter = t.ShowResultsAfter,
                ShowCorrectAnswers = t.ShowCorrectAnswers,
                ShowDetailedFeedback = t.ShowDetailedFeedback,
                TimeLimit = t.TimeLimit,
                IsPublished = t.IsPublished,
                IsPublic = t.IsPublic,
                TotalPoints = t.TotalPoints,
                QuestionCount = t.Questions.Count,
                CreatedAt = t.CreatedAt,
                UserId = t.UserId,
                UserName = t.User.UserName ?? "Unknown",
                Tags = t.Tags.Select(tag => tag.Name).ToList()
            })
            .ToListAsync();

        return Ok(exams);
    }

    /// <summary>
    /// Получить экзамен по ID (детали)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExamDto>> GetExamById(int id)
    {
        var exam = await _context.Exams
            .Include(t => t.User)
            .Include(t => t.Tags)
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (exam == null)
            return NotFound("Экзамен не найден");

        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));
        var isOwnerOrAdmin = exam.UserId == userId || userRoles.Contains("Admin") || userRoles.Contains("Teacher");

        // Проверка доступа: должен быть опубликован или создан текущим пользователем или админом
        if (!exam.IsPublished && !isOwnerOrAdmin)
            return Forbid();

        var examDto = new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Subject = exam.Subject,
            Difficulty = exam.Difficulty,
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

        // Добавляем вопросы и ответы для владельца/админа
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

        return Ok(examDto);
    }

    /// <summary>
    /// Получить экзамен для прохождения (без правильных ответов)
    /// </summary>
    [HttpGet("{id}/take")]
    public async Task<ActionResult<ExamTakingDto>> GetExamForTaking(int id)
    {
        var userId = await GetUserId();

        var exam = await _context.Exams
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Attempts.Where(a => a.UserId == userId))
            .FirstOrDefaultAsync(t => t.Id == id && t.IsPublished);

        if (exam == null)
            return NotFound("Экзамен не найден или не опубликован");

        // Проверка количества попыток
        var attemptsCount = exam.Attempts.Count(a => a.CompletedAt != null);
        if (attemptsCount >= exam.MaxAttempts)
            return BadRequest($"Вы использовали все попытки ({exam.MaxAttempts})");

        var questions = exam.Questions.OrderBy(q => q.Order).ToList();

        // Перемешиваем вопросы если нужно
        if (exam.ShuffleQuestions)
            questions = questions.OrderBy(_ => Guid.NewGuid()).ToList();

        var examDto = new ExamTakingDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Subject = exam.Subject,
            Difficulty = exam.Difficulty,
            TimeLimit = exam.TimeLimit,
            StrictTiming = exam.StrictTiming,
            PassingScore = exam.PassingScore,
            TotalPoints = exam.TotalPoints,
            MaxAttempts = exam.MaxAttempts,
            RemainingAttempts = exam.MaxAttempts - attemptsCount,
            ShuffleQuestions = exam.ShuffleQuestions,
            ShuffleAnswers = exam.ShuffleAnswers,
            Questions = questions.Select(q => new ExamQuestionTakingDto
            {
                Id = q.Id,
                Text = q.Text,
                QuestionType = q.QuestionType,
                Points = q.Points,
                Order = q.Order,
                Answers = (exam.ShuffleAnswers 
                    ? q.Answers.OrderBy(_ => Guid.NewGuid()) 
                    : q.Answers.OrderBy(a => a.Order))
                    .Select(a => new ExamAnswerTakingDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        Order = a.Order
                    }).ToList()
            }).ToList()
        };

        return Ok(examDto);
    }

    /// <summary>
    /// Начать попытку прохождения экзамена
    /// </summary>
    [HttpPost("{id}/attempts/start")]
    public async Task<ActionResult<object>> StartExamAttempt(int id)
    {
        var userId = await GetUserId();

        var exam = await _context.Exams
            .Include(e => e.Attempts.Where(a => a.UserId == userId))
            .FirstOrDefaultAsync(e => e.Id == id && e.IsPublished);

        if (exam == null)
            return NotFound("Экзамен не найден или не опубликован");

        // Проверка количества попыток
        var completedAttempts = exam.Attempts.Count(a => a.CompletedAt != null);
        if (completedAttempts >= exam.MaxAttempts)
            return BadRequest($"Вы использовали все попытки ({exam.MaxAttempts})");

        // Проверка, нет ли незавершенной попытки
        var activeAttempt = exam.Attempts.FirstOrDefault(a => a.CompletedAt == null);
        if (activeAttempt != null)
        {
            return Ok(new { id = activeAttempt.Id });
        }

        // Создаем новую попытку
        var attempt = new UserExamAttempt
        {
            UserId = userId,
            ExamId = id,
            StartedAt = DateTime.UtcNow,
            AttemptNumber = completedAttempts + 1,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        };

        _context.UserExamAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return Ok(new { id = attempt.Id });
    }

    /// <summary>
    /// Отправить ответы на конкретную попытку экзамена
    /// </summary>
    [HttpPost("{id}/attempts/{attemptId}/submit")]
    public async Task<ActionResult> SubmitExamAttempt(int id, int attemptId, [FromBody] SubmitExamAttemptDto submission)
    {
        var userId = await GetUserId();

        var attempt = await _context.UserExamAttempts
            .Include(a => a.Exam)
                .ThenInclude(e => e.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.ExamId == id);

        if (attempt == null)
            return NotFound("Попытка не найдена");

        if (attempt.CompletedAt != null)
            return BadRequest("Эта попытка уже завершена");

        var exam = attempt.Exam;

        // Проверяем ответы и подсчитываем баллы
        int earnedPoints = 0;
        var userAnswers = new List<UserExamAnswer>();

        foreach (var answerSubmission in submission.Answers)
        {
            var question = exam.Questions.FirstOrDefault(q => q.Id == answerSubmission.QuestionId);
            if (question == null) continue;

            // Для множественного выбора проверяем все выбранные ответы
            var correctAnswerIds = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
            var selectedIds = answerSubmission.AnswerIds;

            bool isCorrect = correctAnswerIds.Count == selectedIds.Count && 
                            correctAnswerIds.All(id => selectedIds.Contains(id));

            var pointsEarned = isCorrect ? question.Points : 0;
            earnedPoints += pointsEarned;

            // Сохраняем все выбранные ответы
            foreach (var answerId in selectedIds)
            {
                userAnswers.Add(new UserExamAnswer
                {
                    AttemptId = attemptId,
                    QuestionId = answerSubmission.QuestionId,
                    SelectedAnswerId = answerId,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned,
                    AnsweredAt = DateTime.UtcNow
                });
            }
        }

        // Обновляем попытку
        attempt.Score = earnedPoints;
        attempt.TotalPoints = exam.TotalPoints;
        attempt.Percentage = exam.TotalPoints > 0 ? (double)earnedPoints / exam.TotalPoints * 100 : 0;
        attempt.Passed = attempt.Percentage >= exam.PassingScore;
        attempt.TimeSpent = TimeSpan.FromSeconds(submission.TimeSpent);
        attempt.CompletedAt = DateTime.UtcNow;

        _context.UserExamAnswers.AddRange(userAnswers);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, score = submission.Score });
    }

    /// <summary>
    /// Отправить ответы на экзамен (старый метод, оставлен для обратной совместимости)
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<ExamResultDto>> SubmitExam([FromBody] SubmitExamDto submission)
    {
        var userId = await GetUserId();

        var exam = await _context.Exams
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Attempts.Where(a => a.UserId == userId))
            .FirstOrDefaultAsync(t => t.Id == submission.ExamId);

        if (exam == null)
            return NotFound("Экзамен не найден");

        // Проверка количества попыток
        var completedAttempts = exam.Attempts.Count(a => a.CompletedAt != null);
        if (completedAttempts >= exam.MaxAttempts)
            return BadRequest("Вы использовали все попытки");

        // Создаём попытку
        var attempt = new UserExamAttempt
        {
            UserId = userId,
            ExamId = exam.Id,
            StartedAt = DateTime.UtcNow.Subtract(submission.TimeSpent),
            CompletedAt = DateTime.UtcNow,
            TimeSpent = submission.TimeSpent,
            AttemptNumber = completedAttempts + 1,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };

        // Проверяем ответы и подсчитываем баллы
        int totalScore = 0;
        int totalPoints = exam.TotalPoints;
        var userAnswers = new List<UserExamAnswer>();

        foreach (var answer in submission.Answers)
        {
            var question = exam.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == answer.SelectedAnswerId);
            if (selectedAnswer == null) continue;

            var isCorrect = selectedAnswer.IsCorrect;
            var pointsEarned = isCorrect ? question.Points : 0;
            totalScore += pointsEarned;

            userAnswers.Add(new UserExamAnswer
            {
                Attempt = attempt,
                QuestionId = answer.QuestionId,
                SelectedAnswerId = answer.SelectedAnswerId,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                AnsweredAt = DateTime.UtcNow
            });
        }

        attempt.Score = totalScore;
        attempt.TotalPoints = totalPoints;
        attempt.Percentage = totalPoints > 0 ? (double)totalScore / totalPoints * 100 : 0;
        attempt.Passed = attempt.Percentage >= exam.PassingScore;
        attempt.UserAnswers = userAnswers;

        _context.UserExamAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Формируем результат
        var result = new ExamResultDto
        {
            AttemptId = attempt.Id,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            Percentage = attempt.Percentage,
            Passed = attempt.Passed,
            TimeSpent = attempt.TimeSpent,
            CompletedAt = attempt.CompletedAt.Value,
            AttemptNumber = attempt.AttemptNumber,
            RemainingAttempts = exam.MaxAttempts - attempt.AttemptNumber,
            ShowCorrectAnswers = exam.ShowCorrectAnswers,
            ShowDetailedFeedback = exam.ShowDetailedFeedback
        };

        // Добавляем детальные результаты если разрешено показывать сразу
        if (exam.ShowResultsAfter == "Immediate")
        {
            result.QuestionResults = exam.Questions.Select(q =>
            {
                var userAnswer = userAnswers.FirstOrDefault(ua => ua.QuestionId == q.Id);
                var correctAnswer = q.Answers.FirstOrDefault(a => a.IsCorrect);

                return new ExamQuestionResultDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    Points = q.Points,
                    PointsEarned = userAnswer?.PointsEarned ?? 0,
                    IsCorrect = userAnswer?.IsCorrect ?? false,
                    Explanation = exam.ShowDetailedFeedback ? q.Explanation : null,
                    SelectedAnswerId = userAnswer?.SelectedAnswerId ?? 0,
                    SelectedAnswerText = userAnswer != null 
                        ? q.Answers.FirstOrDefault(a => a.Id == userAnswer.SelectedAnswerId)?.Text ?? ""
                        : "",
                    CorrectAnswerId = exam.ShowCorrectAnswers ? correctAnswer?.Id : null,
                    CorrectAnswerText = exam.ShowCorrectAnswers ? correctAnswer?.Text : null
                };
            }).ToList();
        }

        return Ok(result);
    }

    /// <summary>
    /// Получить статистику экзамена (для преподавателей и админов)
    /// </summary>
    [HttpGet("{id}/stats")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ExamStatsDto>> GetExamStats(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams
            .Include(t => t.Questions)
            .Include(t => t.Attempts)
                .ThenInclude(a => a.User)
            .Include(t => t.Attempts)
                .ThenInclude(a => a.UserAnswers)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (exam == null)
            return NotFound("Экзамен не найден");

        // Проверка доступа
        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        var completedAttempts = exam.Attempts.Where(a => a.CompletedAt != null).ToList();

        var stats = new ExamStatsDto
        {
            ExamId = exam.Id,
            Title = exam.Title,
            TotalAttempts = completedAttempts.Count,
            UniqueStudents = completedAttempts.Select(a => a.UserId).Distinct().Count(),
            AverageScore = completedAttempts.Any() ? completedAttempts.Average(a => a.Percentage) : 0,
            PassRate = completedAttempts.Any() 
                ? (double)completedAttempts.Count(a => a.Passed) / completedAttempts.Count * 100 
                : 0,
            AverageTimeSpent = completedAttempts.Any() 
                ? TimeSpan.FromSeconds(completedAttempts.Average(a => a.TimeSpent.TotalSeconds))
                : TimeSpan.Zero,
            QuestionStats = exam.Questions.Select(q =>
            {
                var allAnswers = completedAttempts
                    .SelectMany(a => a.UserAnswers)
                    .Where(ua => ua.QuestionId == q.Id)
                    .ToList();

                return new ExamQuestionStatsDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    TotalAnswers = allAnswers.Count,
                    CorrectAnswers = allAnswers.Count(ua => ua.IsCorrect),
                    SuccessRate = allAnswers.Any() 
                        ? (double)allAnswers.Count(ua => ua.IsCorrect) / allAnswers.Count * 100 
                        : 0
                };
            }).ToList(),
            RecentAttempts = completedAttempts
                .OrderByDescending(a => a.CompletedAt)
                .Take(10)
                .Select(a => new ExamAttemptSummaryDto
                {
                    AttemptId = a.Id,
                    StudentName = a.User.UserName ?? "Unknown",
                    Score = a.Score,
                    TotalPoints = a.TotalPoints,
                    Percentage = a.Percentage,
                    Passed = a.Passed,
                    TimeSpent = a.TimeSpent,
                    CompletedAt = a.CompletedAt.Value
                })
                .ToList()
        };

        return Ok(stats);
    }

    /// <summary>
    /// Создать новый экзамен
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ExamDto>> CreateExam([FromBody] CreateExamDto dto)
    {
        var userId = await GetUserId();

        var exam = new Exam
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            Difficulty = dto.Difficulty,
            MaxAttempts = dto.MaxAttempts,
            PassingScore = dto.PassingScore,
            IsProctored = dto.IsProctored,
            ShuffleQuestions = dto.ShuffleQuestions,
            ShuffleAnswers = dto.ShuffleAnswers,
            ShowResultsAfter = dto.ShowResultsAfter,
            ShowCorrectAnswers = dto.ShowCorrectAnswers,
            ShowDetailedFeedback = dto.ShowDetailedFeedback,
            TimeLimit = dto.TimeLimit,
            IsPublished = dto.IsPublished,
            IsPublic = dto.IsPublic,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TotalPoints = dto.Questions.Sum(q => q.Points)
        };

        // Добавляем вопросы
        foreach (var questionDto in dto.Questions)
        {
            var question = new ExamQuestion
            {
                Text = questionDto.Text,
                Explanation = questionDto.Explanation,
                Points = questionDto.Points,
                Order = questionDto.Order,
                Exam = exam
            };

            foreach (var answerDto in questionDto.Answers)
            {
                var answer = new ExamAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    Order = answerDto.Order,
                    Question = question
                };
                question.Answers.Add(answer);
            }

            exam.Questions.Add(question);
        }

        // Добавляем теги
        if (dto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            exam.Tags = tags;
        }

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExamById), new { id = exam.Id }, new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Subject = exam.Subject,
            Difficulty = exam.Difficulty,
            MaxAttempts = exam.MaxAttempts,
            PassingScore = exam.PassingScore,
            IsPublished = exam.IsPublished,
            IsPublic = exam.IsPublic,
            TotalPoints = exam.TotalPoints,
            QuestionCount = exam.Questions.Count,
            CreatedAt = exam.CreatedAt,
            UserId = exam.UserId,
            Tags = exam.Tags.Select(t => t.Name).ToList()
        });
    }

    /// <summary>
    /// Обновить экзамен
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult> UpdateExam(int id, [FromBody] CreateExamDto dto)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (exam == null)
            return NotFound("Экзамен не найден");

        // Проверка доступа
        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        // Обновляем поля
        exam.Title = dto.Title;
        exam.Description = dto.Description;
        exam.Subject = dto.Subject;
        exam.Difficulty = dto.Difficulty;
        exam.MaxAttempts = dto.MaxAttempts;
        exam.PassingScore = dto.PassingScore;
        exam.IsProctored = dto.IsProctored;
        exam.ShuffleQuestions = dto.ShuffleQuestions;
        exam.ShuffleAnswers = dto.ShuffleAnswers;
        exam.ShowResultsAfter = dto.ShowResultsAfter;
        exam.ShowCorrectAnswers = dto.ShowCorrectAnswers;
        exam.ShowDetailedFeedback = dto.ShowDetailedFeedback;
        exam.TimeLimit = dto.TimeLimit;
        exam.IsPublished = dto.IsPublished;
        exam.IsPublic = dto.IsPublic;
        exam.UpdatedAt = DateTime.UtcNow;
        exam.TotalPoints = dto.Questions.Sum(q => q.Points);

        // Удаляем старые вопросы
        _context.ExamQuestions.RemoveRange(exam.Questions);

        // Добавляем новые вопросы
        exam.Questions.Clear();
        foreach (var questionDto in dto.Questions)
        {
            var question = new ExamQuestion
            {
                Text = questionDto.Text,
                Explanation = questionDto.Explanation,
                QuestionType = questionDto.QuestionType,
                Points = questionDto.Points,
                Order = questionDto.Order,
                Exam = exam
            };

            foreach (var answerDto in questionDto.Answers)
            {
                var answer = new ExamAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    Order = answerDto.Order,
                    Question = question
                };
                question.Answers.Add(answer);
            }

            exam.Questions.Add(question);
        }

        // Обновляем теги
        exam.Tags.Clear();
        if (dto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            exam.Tags = tags;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Опубликовать экзамен
    /// </summary>
    [HttpPatch("{id}/publish")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult> PublishExam(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
            return NotFound("Экзамен не найден");

        // Проверка доступа
        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        exam.IsPublished = true;
        exam.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { isPublished = exam.IsPublished });
    }

    /// <summary>
    /// Снять экзамен с публикации
    /// </summary>
    [HttpPatch("{id}/unpublish")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult> UnpublishExam(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
            return NotFound("Экзамен не найден");

        // Проверка доступа
        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        exam.IsPublished = false;
        exam.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { isPublished = exam.IsPublished });
    }

    /// <summary>
    /// Удалить экзамен
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult> DeleteExam(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
            return NotFound("Экзамен не найден");

        // Проверка доступа
        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
