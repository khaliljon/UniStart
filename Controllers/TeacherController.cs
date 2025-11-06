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
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")] // Админы и преподаватели
public class TeacherController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeacherController> _logger;

    public TeacherController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TeacherController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Создать публичный тест (доступен всем студентам)
    /// </summary>
    [HttpPost("quizzes/public")]
    public async Task<ActionResult<QuizDto>> CreatePublicQuiz([FromBody] CreateQuizDto dto)
    {
        var userId = GetUserId();

        var quiz = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            TimeLimit = dto.TimeLimit,
            UserId = userId,
            IsPublic = true, // Публичный тест
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Преподаватель создал публичный тест: {Title}", quiz.Title);

        return CreatedAtAction(nameof(QuizzesController.GetQuiz), "Quizzes", new { id = quiz.Id }, new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            Subject = quiz.Subject,
            TimeLimit = quiz.TimeLimit,
            IsPublic = quiz.IsPublic,
            CreatedAt = quiz.CreatedAt
        });
    }

    /// <summary>
    /// Получить все тесты, созданные преподавателем
    /// </summary>
    [HttpGet("quizzes/my")]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetMyQuizzes(
        [FromQuery] bool? isPublic = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var query = _context.Quizzes
            .Where(q => q.UserId == userId);

        if (isPublic.HasValue)
            query = query.Where(q => q.IsPublic == isPublic.Value);

        var quizzes = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                Subject = q.Subject,
                TimeLimit = q.TimeLimit,
                IsPublic = q.IsPublic,
                CreatedAt = q.CreatedAt,
                QuestionCount = q.Questions.Count
            })
            .ToListAsync();

        return Ok(quizzes);
    }

    /// <summary>
    /// Получить статистику по конкретному тесту (сколько раз пройден, средний балл и т.д.)
    /// </summary>
    [HttpGet("quizzes/{quizId}/stats")]
    public async Task<ActionResult<object>> GetQuizStats(int quizId)
    {
        var userId = GetUserId();

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
            return NotFound(new { Message = "Тест не найден" });

        // Только создатель теста может видеть статистику
        if (quiz.UserId != userId)
            return Forbid();

        // Получаем все попытки прохождения этого теста
        var attempts = await _context.UserQuizAttempts
            .Where(qa => qa.QuizId == quizId)
            .ToListAsync();

        if (attempts.Count == 0)
        {
            return Ok(new
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                TotalAttempts = 0,
                UniqueUsers = 0,
                AverageScore = 0.0,
                AveragePercentage = 0.0,
                HighestScore = 0,
                LowestScore = 0
            });
        }

        var totalAttempts = attempts.Count;
        var uniqueUsers = attempts.Select(a => a.UserId).Distinct().Count();
        var averageScore = attempts.Average(a => a.Score);
        var averagePercentage = attempts.Average(a => a.Percentage);
        var highestScore = attempts.Max(a => a.Score);
        var lowestScore = attempts.Min(a => a.Score);

        return Ok(new
        {
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            QuestionCount = quiz.Questions.Count,
            TotalAttempts = totalAttempts,
            UniqueUsers = uniqueUsers,
            AverageScore = Math.Round(averageScore, 2),
            AveragePercentage = Math.Round(averagePercentage, 2),
            HighestScore = highestScore,
            LowestScore = lowestScore,
            PassRate = Math.Round(attempts.Count(a => a.Percentage >= 70) * 100.0 / totalAttempts, 2) // 70% - проходной балл
        });
    }

    /// <summary>
    /// Получить список студентов, которые проходили тесты преподавателя
    /// </summary>
    [HttpGet("students")]
    public async Task<ActionResult<object>> GetStudents(
        [FromQuery] int? quizId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        // Получаем все тесты преподавателя
        var teacherQuizIds = await _context.Quizzes
            .Where(q => q.UserId == userId)
            .Select(q => q.Id)
            .ToListAsync();

        if (teacherQuizIds.Count == 0)
            return Ok(new { Message = "У вас ещё нет тестов", Students = new List<object>() });

        // Получаем попытки по тестам преподавателя
        var query = _context.UserQuizAttempts
            .Where(qa => teacherQuizIds.Contains(qa.QuizId));

        if (quizId.HasValue)
            query = query.Where(qa => qa.QuizId == quizId.Value);

        var attempts = await query
            .Include(qa => qa.User)
            .Include(qa => qa.Quiz)
            .ToListAsync();

        // Группируем по студентам
        var studentStats = attempts
            .GroupBy(qa => qa.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Email = g.First().User.Email,
                UserName = g.First().User.UserName,
                TotalAttempts = g.Count(),
                AverageScore = Math.Round(g.Average(a => a.Score), 2),
                AveragePercentage = Math.Round(g.Average(a => a.Percentage), 2),
                BestScore = g.Max(a => a.Score),
                LastAttemptDate = g.Max(a => a.CompletedAt),
                QuizzesTaken = g.Select(a => a.QuizId).Distinct().Count()
            })
            .OrderByDescending(s => s.AveragePercentage)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            TotalStudents = studentStats.Count,
            Page = page,
            PageSize = pageSize,
            Students = studentStats
        });
    }

    /// <summary>
    /// Получить детальную статистику по конкретному студенту
    /// </summary>
    [HttpGet("students/{studentId}/stats")]
    public async Task<ActionResult<object>> GetStudentStats(string studentId)
    {
        var userId = GetUserId();

        // Получаем информацию о студенте
        var student = await _userManager.FindByIdAsync(studentId);
        if (student == null)
            return NotFound(new { Message = "Студент не найден" });

        // Получаем все тесты преподавателя
        var teacherQuizIds = await _context.Quizzes
            .Where(q => q.UserId == userId)
            .Select(q => q.Id)
            .ToListAsync();

        // Получаем попытки студента по тестам преподавателя
        var attempts = await _context.UserQuizAttempts
            .Include(qa => qa.Quiz)
            .Where(qa => qa.UserId == studentId && teacherQuizIds.Contains(qa.QuizId))
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();

        if (attempts.Count == 0)
        {
            return Ok(new
            {
                StudentId = studentId,
                Email = student.Email,
                Message = "Студент ещё не проходил ваши тесты",
                Attempts = new List<object>()
            });
        }

        var attemptDetails = attempts.Select(a => new
        {
            AttemptId = a.Id,
            QuizId = a.QuizId,
            QuizTitle = a.Quiz.Title,
            Score = a.Score,
            MaxScore = a.MaxScore,
            Percentage = Math.Round(a.Percentage, 2),
            CompletedAt = a.CompletedAt
        }).ToList();

        return Ok(new
        {
            StudentId = studentId,
            Email = student.Email,
            UserName = student.UserName,
            TotalAttempts = attempts.Count,
            AverageScore = Math.Round(attempts.Average(a => a.Score), 2),
            AveragePercentage = Math.Round(attempts.Average(a => a.Percentage), 2),
            BestScore = attempts.Max(a => a.Score),
            QuizzesTaken = attempts.Select(a => a.QuizId).Distinct().Count(),
            Attempts = attemptDetails
        });
    }

    /// <summary>
    /// Получить общую статистику по всем тестам преподавателя
    /// </summary>
    [HttpGet("stats/overview")]
    public async Task<ActionResult<object>> GetOverviewStats()
    {
        var userId = GetUserId();

        var quizzes = await _context.Quizzes
            .Where(q => q.UserId == userId)
            .Include(q => q.Questions)
            .ToListAsync();

        var quizIds = quizzes.Select(q => q.Id).ToList();

        var attempts = await _context.UserQuizAttempts
            .Where(qa => quizIds.Contains(qa.QuizId))
            .ToListAsync();

        var totalQuizzes = quizzes.Count;
        var publicQuizzes = quizzes.Count(q => q.IsPublic);
        var totalQuestions = quizzes.Sum(q => q.Questions.Count);
        var totalAttempts = attempts.Count;
        var uniqueStudents = attempts.Select(a => a.UserId).Distinct().Count();
        var averageScore = attempts.Any() ? Math.Round(attempts.Average(a => a.Percentage), 2) : 0.0;

        return Ok(new
        {
            TotalQuizzes = totalQuizzes,
            PublicQuizzes = publicQuizzes,
            PrivateQuizzes = totalQuizzes - publicQuizzes,
            TotalQuestions = totalQuestions,
            TotalAttempts = totalAttempts,
            UniqueStudents = uniqueStudents,
            AverageStudentScore = averageScore
        });
    }

    /// <summary>
    /// Опубликовать/скрыть тест (переключить IsPublic)
    /// </summary>
    [HttpPatch("quizzes/{quizId}/toggle-public")]
    public async Task<ActionResult> ToggleQuizPublic(int quizId)
    {
        var userId = GetUserId();

        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null)
            return NotFound(new { Message = "Тест не найден" });

        if (quiz.UserId != userId)
            return Forbid();

        quiz.IsPublic = !quiz.IsPublic;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var status = quiz.IsPublic ? "опубликован" : "скрыт";
        _logger.LogInformation("Тест {Title} {Status}", quiz.Title, status);

        return Ok(new
        {
            Message = $"Тест успешно {status}",
            QuizId = quiz.Id,
            IsPublic = quiz.IsPublic
        });
    }

    /// <summary>
    /// Экспорт результатов квиза в CSV
    /// </summary>
    [HttpGet("quizzes/{quizId}/export-results")]
    public async Task<ActionResult> ExportQuizResults(int quizId)
    {
        var userId = GetUserId();

        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == quizId && q.UserId == userId);

        if (quiz == null)
            return NotFound(new { Message = "Тест не найден или у вас нет доступа" });

        var attempts = await _context.UserQuizAttempts
            .Include(qa => qa.User)
            .Where(qa => qa.QuizId == quizId)
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();

        // Создаём CSV
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Email,UserName,Score,MaxScore,Percentage,TimeSpent(sec),CompletedAt");

        foreach (var attempt in attempts)
        {
            csv.AppendLine($"{attempt.User.Email},{attempt.User.UserName},{attempt.Score},{attempt.MaxScore},{attempt.Percentage:F2},{attempt.TimeSpentSeconds},{attempt.CompletedAt:yyyy-MM-dd HH:mm:ss}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"Quiz_{quiz.Id}_{quiz.Title}_Results_{DateTime.UtcNow:yyyyMMdd}.csv";

        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Получить детальный отчёт по конкретной попытке студента
    /// </summary>
    [HttpGet("quiz-attempts/{attemptId}/detailed")]
    public async Task<ActionResult<object>> GetAttemptDetails(int attemptId)
    {
        var userId = GetUserId();

        var attempt = await _context.UserQuizAttempts
            .Include(qa => qa.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Answers)
            .Include(qa => qa.User)
            .FirstOrDefaultAsync(qa => qa.Id == attemptId);

        if (attempt == null)
            return NotFound(new { Message = "Попытка не найдена" });

        // Проверяем, что это тест преподавателя
        if (attempt.Quiz.UserId != userId)
            return Forbid();

        // Парсим ответы пользователя
        var userAnswers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, List<int>>>(attempt.UserAnswersJson);

        var questionDetails = attempt.Quiz.Questions.Select(q =>
        {
            var correctAnswerIds = q.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
            var userAnswerIds = userAnswers?.ContainsKey(q.Id) == true ? userAnswers[q.Id] : new List<int>();
            
            var isCorrect = correctAnswerIds.OrderBy(x => x).SequenceEqual(userAnswerIds.OrderBy(x => x));

            return new
            {
                QuestionId = q.Id,
                QuestionText = q.Text,
                QuestionType = q.QuestionType,
                Points = q.Points,
                IsCorrect = isCorrect,
                CorrectAnswers = q.Answers.Where(a => a.IsCorrect).Select(a => new { a.Id, a.Text }),
                UserAnswers = q.Answers.Where(a => userAnswerIds.Contains(a.Id)).Select(a => new { a.Id, a.Text }),
                Explanation = q.Explanation
            };
        }).ToList();

        return Ok(new
        {
            AttemptId = attempt.Id,
            Student = new
            {
                attempt.User.Id,
                attempt.User.Email,
                attempt.User.UserName
            },
            Quiz = new
            {
                attempt.Quiz.Id,
                attempt.Quiz.Title,
                attempt.Quiz.Subject
            },
            Score = attempt.Score,
            MaxScore = attempt.MaxScore,
            Percentage = attempt.Percentage,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            CompletedAt = attempt.CompletedAt,
            Questions = questionDetails
        });
    }

    /// <summary>
    /// Создать приватный набор карточек (доступен только преподавателю)
    /// </summary>
    [HttpPost("flashcard-sets/private")]
    public async Task<ActionResult<FlashcardSetDto>> CreatePrivateFlashcardSet([FromBody] CreateFlashcardSetDto dto)
    {
        var userId = GetUserId();

        var flashcardSet = new FlashcardSet
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            IsPublic = false, // Приватный набор
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FlashcardSets.Add(flashcardSet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Преподаватель создал приватный набор карточек: {Title}", flashcardSet.Title);

        return CreatedAtAction("GetFlashcardSet", "Flashcards", new { id = flashcardSet.Id }, new FlashcardSetDto
        {
            Id = flashcardSet.Id,
            Title = flashcardSet.Title,
            Description = flashcardSet.Description,
            Subject = flashcardSet.Subject,
            IsPublic = flashcardSet.IsPublic,
            CreatedAt = flashcardSet.CreatedAt,
            UpdatedAt = flashcardSet.UpdatedAt,
            CardCount = 0,
            TotalCards = 0,
            CardsToReview = 0
        });
    }

    /// <summary>
    /// Получить все свои наборы карточек (публичные и приватные)
    /// </summary>
    [HttpGet("flashcard-sets/my")]
    public async Task<ActionResult<IEnumerable<FlashcardSetDto>>> GetMyFlashcardSets(
        [FromQuery] bool? isPublic = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var query = _context.FlashcardSets
            .Include(fs => fs.Flashcards)
            .Where(fs => fs.UserId == userId);

        if (isPublic.HasValue)
            query = query.Where(fs => fs.IsPublic == isPublic.Value);

        var flashcardSets = await query
            .OrderByDescending(fs => fs.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(fs => new FlashcardSetDto
            {
                Id = fs.Id,
                Title = fs.Title,
                Description = fs.Description,
                Subject = fs.Subject,
                IsPublic = fs.IsPublic,
                CreatedAt = fs.CreatedAt,
                UpdatedAt = fs.UpdatedAt,
                CardCount = fs.Flashcards.Count,
                TotalCards = fs.Flashcards.Count,
                CardsToReview = fs.Flashcards.Count(f => f.NextReviewDate == null || f.NextReviewDate <= DateTime.UtcNow)
            })
            .ToListAsync();

        return Ok(flashcardSets);
    }
}
