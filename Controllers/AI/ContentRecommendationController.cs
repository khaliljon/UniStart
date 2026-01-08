using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Services.AI;
using System.Security.Claims;

namespace UniStart.Controllers.AI;

/// <summary>
/// Контроллер для AI рекомендаций контента
/// </summary>
[ApiController]
[Route("api/ai/content-recommendations")]
[Authorize]
public class ContentRecommendationController : ControllerBase
{
    private readonly IContentRecommendationService _contentService;
    private readonly ILogger<ContentRecommendationController> _logger;

    public ContentRecommendationController(
        IContentRecommendationService contentService,
        ILogger<ContentRecommendationController> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    /// <summary>
    /// Получить персонализированные рекомендации квизов
    /// </summary>
    [HttpGet("quizzes/recommended")]
    public async Task<IActionResult> GetRecommendedQuizzes([FromQuery] int count = 5)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var quizIds = await _contentService.RecommendQuizzesForWeaknesses(userId, count);

            return Ok(new
            {
                total = quizIds.Count,
                quizIds,
                message = "Квизы подобраны на основе анализа ваших слабых сторон"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендаций квизов");
            return StatusCode(500, new { message = "Ошибка при получении рекомендаций" });
        }
    }

    /// <summary>
    /// Получить рекомендации экзаменов
    /// </summary>
    [HttpGet("exams/recommended")]
    public async Task<IActionResult> GetRecommendedExams([FromQuery] int count = 3)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var examIds = await _contentService.RecommendExamsForGoals(userId, count);

            return Ok(new
            {
                total = examIds.Count,
                examIds,
                message = "Экзамены подобраны на основе ваших целей"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендаций экзаменов");
            return StatusCode(500, new { message = "Ошибка при получении рекомендаций" });
        }
    }

    /// <summary>
    /// Получить рекомендации наборов flashcards
    /// </summary>
    [HttpGet("flashcards/recommended")]
    public async Task<IActionResult> GetRecommendedFlashcardSets([FromQuery] int count = 5)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var setIds = await _contentService.RecommendFlashcardSets(userId, count);

            return Ok(new
            {
                total = setIds.Count,
                flashcardSetIds = setIds,
                message = "Наборы карточек подобраны для улучшения слабых тем"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендаций наборов карточек");
            return StatusCode(500, new { message = "Ошибка при получении рекомендаций" });
        }
    }

    /// <summary>
    /// Получить следующую рекомендованную тему для изучения
    /// </summary>
    [HttpGet("next-topic")]
    public async Task<IActionResult> GetNextTopic()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var nextTopic = await _contentService.GetNextTopicToStudy(userId);

            if (string.IsNullOrEmpty(nextTopic))
            {
                return Ok(new
                {
                    topic = (string?)null,
                    message = "Вы изучили все доступные темы. Отличная работа!"
                });
            }

            return Ok(new
            {
                topic = nextTopic,
                message = $"Рекомендуем начать изучение темы: {nextTopic}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при определении следующей темы");
            return StatusCode(500, new { message = "Ошибка при определении темы" });
        }
    }

    /// <summary>
    /// Получить персонализированные советы по обучению
    /// </summary>
    [HttpGet("tips")]
    public async Task<IActionResult> GetPersonalizedTips()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var tips = await _contentService.GetPersonalizedTips(userId);

            return Ok(new
            {
                total = tips.Count,
                tips,
                generatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении персональных советов");
            return StatusCode(500, new { message = "Ошибка при получении советов" });
        }
    }

    /// <summary>
    /// Получить комплексную панель рекомендаций
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetRecommendationsDashboard()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var quizzes = await _contentService.RecommendQuizzesForWeaknesses(userId, 3);
            var exams = await _contentService.RecommendExamsForGoals(userId, 2);
            var flashcards = await _contentService.RecommendFlashcardSets(userId, 3);
            var nextTopic = await _contentService.GetNextTopicToStudy(userId);
            var tips = await _contentService.GetPersonalizedTips(userId);

            return Ok(new
            {
                recommendedQuizzes = quizzes,
                recommendedExams = exams,
                recommendedFlashcardSets = flashcards,
                nextTopicToStudy = nextTopic,
                personalizedTips = tips,
                generatedAt = DateTime.UtcNow,
                message = "Персональная панель рекомендаций создана на основе AI анализа"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании панели рекомендаций");
            return StatusCode(500, new { message = "Ошибка при создании панели" });
        }
    }
}
