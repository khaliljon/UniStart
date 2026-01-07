using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Controllers.Teacher;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
public class TeacherQuizzesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeacherQuizzesController> _logger;

    public TeacherQuizzesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TeacherQuizzesController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Создать публичный квиз (доступен всем студентам)
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
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Преподаватель создал публичный квиз: {Title}", quiz.Title);

        return CreatedAtAction("GetQuiz", "QuizzesQuery", new { id = quiz.Id }, new QuizDto
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
    /// Получить все квизы, созданные преподавателем
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
    /// Получить статистику по конкретному квизу
    /// </summary>
    [HttpGet("quizzes/{quizId}/stats")]
    public async Task<ActionResult<object>> GetQuizStats(int quizId)
    {
        var userId = GetUserId();

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
            return NotFound(new { Message = "Квиз не найден" });

        if (quiz.UserId != userId)
            return Forbid();

        var attempts = await _context.UserQuizAttempts
            .Where(qa => qa.QuizId == quizId && qa.CompletedAt != null)
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
            PassRate = Math.Round(attempts.Count(a => a.Percentage >= 70) * 100.0 / totalAttempts, 2)
        });
    }

    /// <summary>
    /// Опубликовать/скрыть квиз
    /// </summary>
    [HttpPatch("quizzes/{quizId}/publish")]
    public async Task<ActionResult> ToggleQuizPublic(int quizId)
    {
        var userId = GetUserId();

        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null)
            return NotFound(new { Message = "Квиз не найден" });

        if (quiz.UserId != userId)
            return Forbid();

        quiz.IsPublic = !quiz.IsPublic;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var status = quiz.IsPublic ? "опубликован" : "скрыт";
        _logger.LogInformation("Квиз {Title} {Status}", quiz.Title, status);

        return Ok(new
        {
            Message = $"Квиз успешно {status}",
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
            return NotFound(new { Message = "Квиз не найден или у вас нет доступа" });

        var attempts = await _context.UserQuizAttempts
            .Include(qa => qa.User)
            .Where(qa => qa.QuizId == quizId)
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Email,UserName,Score,MaxScore,Percentage,TimeSpent(sec),CompletedAt");

        foreach (var attempt in attempts)
        {
            csv.AppendLine($"{attempt.User.Email},{attempt.User.UserName},{attempt.Score},{attempt.MaxScore},{attempt.Percentage:F2},{attempt.TimeSpentSeconds},{attempt.CompletedAt:yyyy-MM-dd HH:mm:ss}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"Quiz_{quiz.Id}_{quiz.Title}_Results_{DateTime.UtcNow:yyyyMMdd}.csv";

        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Получить детальный отчёт по попытке студента
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

        if (attempt.Quiz.UserId != userId)
            return Forbid();

        var userAnswers = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(attempt.UserAnswersJson);

        var questionDetails = attempt.Quiz.Questions.Select(q =>
        {
            var correctAnswerIds = q.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
            var userAnswerIds = userAnswers?.ContainsKey(q.Id) == true ? userAnswers[q.Id] : new List<int>();
            
            var isCorrect = correctAnswerIds.OrderBy(x => x).SequenceEqual(userAnswerIds.OrderBy(x => x));

            return new
            {
                QuestionId = q.Id,
                QuestionText = q.Text,
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
    /// Получить общую статистику по всем квизам преподавателя
    /// </summary>
    [HttpGet("quizzes/stats/overview")]
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
}
