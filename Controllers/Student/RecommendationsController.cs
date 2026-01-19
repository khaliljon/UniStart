using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniStart.Services;
using UniStart.Models.Flashcards;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;

namespace UniStart.Controllers.Student;

[Authorize(Roles = "Student")]
[Route("api/student/recommendations")]
[ApiController]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<RecommendationsController> _logger;

    public RecommendationsController(
        IRecommendationService recommendationService,
        ILogger<RecommendationsController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    /// <summary>
    /// Получить рекомендованные наборы карточек
    /// </summary>
    /// <param name="count">Количество рекомендаций (по умолчанию 10)</param>
    [HttpGet("flashcards")]
    public async Task<ActionResult<IEnumerable<object>>> GetRecommendedFlashcardSets([FromQuery] int count = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var sets = await _recommendationService.GetRecommendedFlashcardSetsAsync(userId, count);

        var result = sets.Select(fs => new
        {
            fs.Id,
            fs.Title,
            fs.Description,
            fs.IsPublic,
            fs.CreatedAt,
            CardCount = fs.Flashcards.Count,
            Subjects = fs.Subjects.Select(s => new { s.Id, s.Name }).ToList()
        });

        return Ok(result);
    }

    /// <summary>
    /// Получить рекомендованные квизы
    /// </summary>
    /// <param name="count">Количество рекомендаций (по умолчанию 10)</param>
    [HttpGet("quizzes")]
    public async Task<ActionResult<IEnumerable<object>>> GetRecommendedQuizzes([FromQuery] int count = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var quizzes = await _recommendationService.GetRecommendedQuizzesAsync(userId, count);

        var result = quizzes.Select(q => new
        {
            q.Id,
            q.Title,
            q.Description,
            q.Difficulty,
            q.TimeLimit,
            q.IsPublic,
            q.CreatedAt,
            QuestionCount = q.Questions.Count,
            Subjects = q.Subjects.Select(s => new { s.Id, s.Name }).ToList()
        });

        return Ok(result);
    }

    /// <summary>
    /// Получить рекомендованные экзамены
    /// </summary>
    /// <param name="count">Количество рекомендаций (по умолчанию 10)</param>
    [HttpGet("exams")]
    public async Task<ActionResult<IEnumerable<object>>> GetRecommendedExams([FromQuery] int count = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var exams = await _recommendationService.GetRecommendedExamsAsync(userId, count);

        var result = exams.Select(e => new
        {
            e.Id,
            e.Title,
            e.Description,
            e.TimeLimit,
            e.CreatedAt,
            QuestionCount = e.Questions.Count,
            MaxScore = e.Questions.Sum(q => q.Points),
            Subjects = e.Subjects.Select(s => new { s.Id, s.Name }).ToList()
        });

        return Ok(result);
    }

    /// <summary>
    /// Получить все рекомендации сразу (микс контента)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<object>> GetAllRecommendations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var flashcards = await _recommendationService.GetRecommendedFlashcardSetsAsync(userId, 5);
        var quizzes = await _recommendationService.GetRecommendedQuizzesAsync(userId, 5);
        var exams = await _recommendationService.GetRecommendedExamsAsync(userId, 3);

        var result = new
        {
            flashcards = flashcards.Select(fs => new
            {
                fs.Id,
                fs.Title,
                fs.Description,
                CardCount = fs.Flashcards.Count,
                Subjects = fs.Subjects.Select(s => s.Name).ToList()
            }),
            quizzes = quizzes.Select(q => new
            {
                q.Id,
                q.Title,
                q.Description,
                q.Difficulty,
                QuestionCount = q.Questions.Count,
                Subjects = q.Subjects.Select(s => s.Name).ToList()
            }),
            exams = exams.Select(e => new
            {
                e.Id,
                e.Title,
                e.Description,
                QuestionCount = e.Questions.Count,
                Subjects = e.Subjects.Select(s => s.Name).ToList()
            })
        };

        _logger.LogInformation("Generated mixed recommendations for user {UserId}: {FlashcardCount} flashcard sets, {QuizCount} quizzes, {ExamCount} exams",
            userId, flashcards.Count(), quizzes.Count(), exams.Count());

        return Ok(result);
    }
}
