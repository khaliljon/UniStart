using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;

namespace UniStart.Controllers.Quizzes;

/// <summary>
/// Контроллер для прохождения квизов и работы с попытками
/// </summary>
[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesAttemptController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuizzesAttemptController> _logger;

    public QuizzesAttemptController(
        ApplicationDbContext context,
        ILogger<QuizzesAttemptController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Начать попытку прохождения квиза
    /// </summary>
    [HttpPost("{id}/attempts/start")]
    public async Task<ActionResult<UserQuizAttempt>> StartQuizAttempt(int id)
    {
        var userId = GetUserId()!;

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id && q.IsPublished);

        if (quiz == null)
            return NotFound("Quiz not found or not published");

        var attempt = new UserQuizAttempt
        {
            UserId = userId,
            QuizId = id,
            StartedAt = DateTime.UtcNow,
            Score = 0,
            MaxScore = quiz.Questions.Sum(q => q.Points)
        };

        _context.UserQuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return Ok(new { attemptId = attempt.Id, startedAt = attempt.StartedAt });
    }

    /// <summary>
    /// Отправить ответы на тест и получить результаты
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<QuizResultDto>> SubmitQuiz(SubmitQuizDto dto)
    {
        var userId = GetUserId()!;
        
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qu => qu.Answers)
            .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

        if (quiz == null)
            return NotFound("Quiz not found");

        // Проверка ответов и подсчет результатов
        var questionResults = new List<QuizQuestionResultDto>();
        int totalScore = 0;
        int maxScore = quiz.Questions.Sum(q => q.Points);

        foreach (var QuizQuestion in quiz.Questions)
        {
            var correctAnswerIds = QuizQuestion.Answers
                .Where(a => a.IsCorrect)
                .Select(a => a.Id)
                .OrderBy(id => id)
                .ToList();

            var userAnswerIds = dto.UserAnswers.ContainsKey(QuizQuestion.Id)
                ? dto.UserAnswers[QuizQuestion.Id].OrderBy(id => id).ToList()
                : new List<int>();

            bool isCorrect = correctAnswerIds.SequenceEqual(userAnswerIds);
            int pointsEarned = isCorrect ? QuizQuestion.Points : 0;
            totalScore += pointsEarned;

            questionResults.Add(new QuizQuestionResultDto
            {
                QuestionId = QuizQuestion.Id,
                QuestionText = QuizQuestion.Text,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                MaxPoints = QuizQuestion.Points,
                CorrectAnswerIds = correctAnswerIds,
                UserAnswerIds = userAnswerIds,
                Explanation = QuizQuestion.Explanation
            });
        }

        // Сохраняем попытку пользователя
        var attempt = new UserQuizAttempt
        {
            UserId = userId,
            QuizId = dto.QuizId,
            Score = totalScore,
            MaxScore = maxScore,
            Percentage = maxScore > 0 ? (double)totalScore / maxScore * 100 : 0,
            TimeSpentSeconds = dto.TimeSpentSeconds,
            CompletedAt = DateTime.UtcNow,
            UserAnswersJson = JsonSerializer.Serialize(dto.UserAnswers)
        };

        _context.UserQuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Создаем записи UserQuizAnswer для каждого ответа
        var userQuizAnswers = new List<UserQuizAnswer>();
        foreach (var question in quiz.Questions)
        {
            if (dto.UserAnswers.TryGetValue(question.Id, out var userAnswerIds) && userAnswerIds.Any())
            {
                var correctAnswerIds = question.Answers
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .OrderBy(id => id)
                    .ToList();

                var sortedUserAnswerIds = userAnswerIds.OrderBy(id => id).ToList();
                bool isCorrect = correctAnswerIds.SequenceEqual(sortedUserAnswerIds);
                int pointsEarned = isCorrect ? question.Points : 0;

                foreach (var answerId in userAnswerIds)
                {
                    var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == answerId);
                    if (selectedAnswer != null)
                    {
                        userQuizAnswers.Add(new UserQuizAnswer
                        {
                            AttemptId = attempt.Id,
                            QuestionId = question.Id,
                            SelectedAnswerId = answerId,
                            IsCorrect = isCorrect && correctAnswerIds.Contains(answerId),
                            PointsEarned = userAnswerIds.IndexOf(answerId) == 0 ? pointsEarned : 0,
                            AnsweredAt = DateTime.UtcNow
                        });
                    }
                }
            }
            else
            {
                userQuizAnswers.Add(new UserQuizAnswer
                {
                    AttemptId = attempt.Id,
                    QuestionId = question.Id,
                    SelectedAnswerId = null,
                    IsCorrect = false,
                    PointsEarned = 0,
                    AnsweredAt = DateTime.UtcNow
                });
            }
        }

        _context.UserQuizAnswers.AddRange(userQuizAnswers);
        await _context.SaveChangesAsync();

        var result = new QuizResultDto
        {
            Score = totalScore,
            MaxScore = maxScore,
            Percentage = attempt.Percentage,
            TimeSpentSeconds = dto.TimeSpentSeconds,
            QuestionResults = questionResults
        };

        return Ok(result);
    }

    /// <summary>
    /// Получить историю попыток текущего пользователя по тесту
    /// </summary>
    [HttpGet("{quizId}/attempts")]
    public async Task<ActionResult<List<UserQuizAttempt>>> GetQuizAttempts(int quizId)
    {
        var userId = GetUserId()!;
        
        var attempts = await _context.UserQuizAttempts
            .Where(a => a.QuizId == quizId && a.UserId == userId)
            .OrderByDescending(a => a.CompletedAt)
            .Take(10)
            .ToListAsync();

        return Ok(attempts);
    }

    /// <summary>
    /// Получить подробную статистику по тесту для страницы статистики (только для владельца теста)
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult> GetQuizStats(int id)
    {
        var userId = GetUserId()!;
        
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qu => qu.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
            
        if (quiz == null)
            return NotFound("Quiz not found");

        // Проверка доступа
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        if (quiz.UserId != userId && !userRoles.Contains("Admin") && !userRoles.Contains("Teacher"))
            return Forbid();
        
        var attempts = await _context.UserQuizAttempts
            .Where(a => a.QuizId == id && a.CompletedAt != null)
            .OrderByDescending(a => a.CompletedAt)
            .ToListAsync();

        if (!attempts.Any())
        {
            return Ok(new
            {
                quizId = id,
                quizTitle = quiz.Title,
                totalAttempts = 0,
                averageScore = 0.0,
                averageTimeSpent = 0,
                passRate = 0.0,
                questionStats = new List<object>(),
                recentAttempts = new List<object>()
            });
        }

        var attemptIds = attempts.Select(a => a.Id).ToList();
        var userAnswers = await _context.UserQuizAnswers
            .Where(a => attemptIds.Contains(a.AttemptId))
            .GroupBy(a => new { a.AttemptId, a.QuestionId })
            .ToListAsync();

        var questionStats = new List<object>();
        foreach (var QuizQuestion in quiz.Questions.OrderBy(q => q.OrderIndex))
        {
            var questionAnswers = userAnswers
                .Where(g => g.Key.QuestionId == QuizQuestion.Id)
                .ToList();

            int totalAnswers = questionAnswers.Count;
            int correctAnswers = questionAnswers.Count(answerGroup => 
                answerGroup.Any(a => a.IsCorrect && a.PointsEarned > 0));

            questionStats.Add(new
            {
                questionId = QuizQuestion.Id,
                questionText = QuizQuestion.Text,
                correctAnswers,
                totalAnswers,
                successRate = totalAnswers > 0 ? (double)correctAnswers / totalAnswers * 100 : 0
            });
        }

        var userIds = attempts.Select(a => a.UserId).Distinct().ToList();
        var usersDict = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var recentAttempts = attempts.Take(10).Select(a => 
        {
            var user = usersDict.TryGetValue(a.UserId, out var u) ? u : null;
            return new
            {
                id = a.Id,
                studentName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                score = a.Score,
                maxScore = a.MaxScore,
                percentage = a.Percentage,
                timeSpent = a.TimeSpentSeconds,
                completedAt = a.CompletedAt
            };
        }).ToList();

        var stats = new
        {
            quizId = id,
            quizTitle = quiz.Title,
            totalAttempts = attempts.Count,
            averageScore = attempts.Average(a => a.Percentage),
            averageTimeSpent = (int)attempts.Average(a => a.TimeSpentSeconds),
            passRate = attempts.Count(a => a.Percentage >= 50) * 100.0 / attempts.Count,
            questionStats,
            recentAttempts
        };

        return Ok(stats);
    }

    /// <summary>
    /// Получить статистику по тесту (только для владельца теста)
    /// </summary>
    [HttpGet("{quizId}/statistics")]
    public async Task<ActionResult> GetQuizStatistics(int quizId)
    {
        var userId = GetUserId()!;
        
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == quizId && q.UserId == userId);
            
        if (quiz == null)
            return NotFound("Quiz not found or access denied");
        
        var attempts = await _context.UserQuizAttempts
            .Where(a => a.QuizId == quizId && a.CompletedAt != null)
            .ToListAsync();

        if (!attempts.Any())
            return Ok(new { message = "No attempts found" });

        var stats = new
        {
            TotalAttempts = attempts.Count,
            AverageScore = attempts.Average(a => a.Percentage),
            HighestScore = attempts.Max(a => a.Percentage),
            LowestScore = attempts.Min(a => a.Percentage),
            AverageTime = attempts.Average(a => a.TimeSpentSeconds)
        };

        return Ok(stats);
    }
}
