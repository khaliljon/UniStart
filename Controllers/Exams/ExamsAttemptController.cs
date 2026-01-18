using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Controllers.Exams;

/// <summary>
/// Контроллер для прохождения экзаменов и работы с попытками
/// </summary>
[ApiController]
[Route("api/exams")]
[Authorize]
public class ExamsAttemptController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ExamsAttemptController> _logger;

    public ExamsAttemptController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ExamsAttemptController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<string> GetUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? throw new UnauthorizedAccessException("Пользователь не авторизован");
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

        var completedAttempts = exam.Attempts.Count(a => a.CompletedAt != null);
        if (completedAttempts >= exam.MaxAttempts)
            return BadRequest($"Вы использовали все попытки ({exam.MaxAttempts})");

        var activeAttempt = exam.Attempts.FirstOrDefault(a => a.CompletedAt == null);
        if (activeAttempt != null)
        {
            return Ok(new { id = activeAttempt.Id });
        }

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
        int earnedPoints = 0;
        var userAnswers = new List<UserExamAnswer>();

        foreach (var answerSubmission in submission.Answers)
        {
            var question = exam.Questions.FirstOrDefault(q => q.Id == answerSubmission.QuestionId);
            if (question == null) continue;

            var correctAnswerIds = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).OrderBy(x => x).ToList();
            var selectedIds = answerSubmission.AnswerIds.OrderBy(x => x).ToList();

            bool isCorrect = correctAnswerIds.Count == selectedIds.Count && 
                            correctAnswerIds.SequenceEqual(selectedIds);

            var pointsEarned = isCorrect ? question.Points : 0;
            earnedPoints += pointsEarned;

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

        attempt.Score = earnedPoints;
        attempt.MaxScore = exam.MaxScore;
        attempt.Percentage = exam.MaxScore > 0 ? (double)earnedPoints / exam.MaxScore * 100 : 0;
        attempt.Passed = attempt.Percentage >= exam.PassingScore;
        attempt.TimeSpentSeconds = submission.TimeSpent;
        attempt.CompletedAt = DateTime.UtcNow;

        _context.UserExamAnswers.AddRange(userAnswers);
        await _context.SaveChangesAsync();

        return Ok(new { 
            success = true, 
            score = (int)attempt.Percentage,
            earnedPoints = earnedPoints,
            maxScore = exam.MaxScore,
            passed = attempt.Passed
        });
    }

    /// <summary>
    /// Отправить ответы на экзамен (старый метод для обратной совместимости)
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

        var completedAttempts = exam.Attempts.Count(a => a.CompletedAt != null);
        if (completedAttempts >= exam.MaxAttempts)
            return BadRequest("Вы использовали все попытки");

        var attempt = new UserExamAttempt
        {
            UserId = userId,
            ExamId = exam.Id,
            StartedAt = DateTime.UtcNow.AddSeconds(-submission.TimeSpent),
            CompletedAt = DateTime.UtcNow,
            TimeSpentSeconds = submission.TimeSpent,
            AttemptNumber = completedAttempts + 1,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };

        int totalScore = 0;
        int maxScore = exam.MaxScore;
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
        attempt.MaxScore = maxScore;
        attempt.Percentage = maxScore > 0 ? (double)totalScore / maxScore * 100 : 0;
        attempt.Passed = attempt.Percentage >= exam.PassingScore;
        attempt.UserAnswers = userAnswers;

        _context.UserExamAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        var result = new ExamResultDto
        {
            AttemptId = attempt.Id,
            Score = attempt.Score,
            MaxScore = attempt.MaxScore,
            Percentage = attempt.Percentage,
            Passed = attempt.Passed,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            CompletedAt = attempt.CompletedAt.Value,
            AttemptNumber = attempt.AttemptNumber,
            RemainingAttempts = exam.MaxAttempts - attempt.AttemptNumber,
            ShowCorrectAnswers = exam.ShowCorrectAnswers,
            ShowDetailedFeedback = exam.ShowDetailedFeedback
        };

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
    /// Получить статистику по экзамену (для владельца)
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult> GetExamStats(int id)
    {
        var userId = await GetUserId();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var userRoles = await _userManager.GetRolesAsync(user);

        var exam = await _context.Exams
            .Include(e => e.Attempts)
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
            return NotFound("Экзамен не найден");

        if (exam.UserId != userId && !userRoles.Contains("Admin") && !userRoles.Contains("Teacher"))
            return Forbid();

        var attempts = exam.Attempts.Where(a => a.CompletedAt != null).ToList();

        if (!attempts.Any())
        {
            return Ok(new
            {
                examId = id,
                examTitle = exam.Title,
                totalAttempts = 0,
                averageScore = 0.0,
                averageTimeSpent = 0,
                passRate = 0.0,
                questionStats = new List<object>(),
                recentAttempts = new List<object>()
            });
        }

        // Статистика по вопросам
        var attemptIds = attempts.Select(a => a.Id).ToList();
        var userAnswers = await _context.UserExamAnswers
            .Where(a => attemptIds.Contains(a.AttemptId))
            .GroupBy(a => new { a.AttemptId, a.QuestionId })
            .ToListAsync();

        var questionStats = new List<object>();
        foreach (var examQuestion in exam.Questions.OrderBy(q => q.OrderIndex))
        {
            var questionAnswers = userAnswers
                .Where(g => g.Key.QuestionId == examQuestion.Id)
                .ToList();

            int totalAnswers = questionAnswers.Count;
            int correctAnswers = questionAnswers.Count(answerGroup => 
                answerGroup.Any(a => a.IsCorrect && a.PointsEarned > 0));

            questionStats.Add(new
            {
                questionId = examQuestion.Id,
                questionText = examQuestion.Text,
                correctAnswers,
                totalAnswers,
                successRate = totalAnswers > 0 ? (double)correctAnswers / totalAnswers * 100 : 0
            });
        }

        // Последние попытки
        var userIds = attempts.Select(a => a.UserId).Distinct().ToList();
        var usersDict = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var recentAttempts = attempts
            .OrderByDescending(a => a.CompletedAt)
            .Take(10)
            .Select(a => 
            {
                var attemptUser = usersDict.TryGetValue(a.UserId, out var u) ? u : null;
                return new
                {
                    id = a.Id,
                    studentName = attemptUser != null ? $"{attemptUser.FirstName} {attemptUser.LastName}".Trim() : "Unknown",
                    score = a.Score,
                    maxScore = a.MaxScore,
                    percentage = a.Percentage,
                    timeSpent = a.TimeSpentSeconds,
                    completedAt = a.CompletedAt
                };
            }).ToList();

        var stats = new
        {
            examId = id,
            examTitle = exam.Title,
            totalAttempts = attempts.Count,
            averageScore = attempts.Average(a => a.Percentage),
            averageTimeSpent = (int)attempts.Average(a => a.TimeSpentSeconds),
            passRate = attempts.Count(a => a.Passed) * 100.0 / attempts.Count,
            questionStats,
            recentAttempts
        };

        return Ok(stats);
    }
}
