using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Services.AI;
using System.Security.Claims;
using UniStart.Data;
using Microsoft.EntityFrameworkCore;

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
    private readonly ApplicationDbContext _context;

    public ContentRecommendationController(
        IContentRecommendationService contentService,
        ILogger<ContentRecommendationController> logger,
        ApplicationDbContext context)
    {
        _contentService = contentService;
        _logger = logger;
        _context = context;
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
            
            // Загружаем полные данные о квизах
            var quizzes = await _context.Quizzes
                .Where(q => quizIds.Contains(q.Id))
                .Select(q => new
                {
                    id = q.Id,
                    title = q.Title,
                    subject = q.Subject,
                    difficulty = q.Difficulty,
                    recommendationReason = "Рекомендовано для улучшения знаний по слабым темам",
                    estimatedDuration = q.TimeLimit,
                    questionsCount = q.Questions.Count
                })
                .ToListAsync();

            return Ok(quizzes);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан для рекомендаций квизов");
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения операции получения рекомендаций квизов");
            return StatusCode(500, new { message = "Не удалось подобрать квизы" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при получении рекомендаций квизов");
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
            
            // Загружаем полные данные об экзаменах
            var exams = await _context.Exams
                .Where(e => examIds.Contains(e.Id))
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    subject = e.Subjects.FirstOrDefault() != null ? e.Subjects.First().Name : "Общий",
                    difficulty = e.Difficulty,
                    recommendationReason = "Рекомендовано на основе ваших целей",
                    duration = e.TimeLimit,
                    questionsCount = e.Questions.Count
                })
                .ToListAsync();

            return Ok(exams);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан для рекомендаций экзаменов");
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения операции получения рекомендаций экзаменов");
            return StatusCode(500, new { message = "Не удалось подобрать экзамены" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при получении рекомендаций экзаменов");
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
            
            // Загружаем полные данные о наборах карточек
            var flashcardSets = await _context.FlashcardSets
                .Where(f => setIds.Contains(f.Id))
                .Select(f => new
                {
                    id = f.Id,
                    title = f.Title,
                    subject = f.Subject,
                    cardsCount = f.Flashcards.Count,
                    recommendationReason = "Рекомендовано для улучшения знаний",
                    estimatedStudyTime = f.Flashcards.Count * 2 // примерно 2 минуты на карточку
                })
                .ToListAsync();

            return Ok(flashcardSets);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан для рекомендаций flashcards");
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения операции получения рекомендаций flashcards");
            return StatusCode(500, new { message = "Не удалось подобрать наборы карточек" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при получении рекомендаций наборов карточек");
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
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан для определения следующей темы");
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения операции определения следующей темы");
            return StatusCode(500, new { message = "Не удалось определить следующую тему" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при определении следующей темы");
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
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан для получения персональных советов");
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения операции получения персональных советов");
            return StatusCode(500, new { message = "Не удалось сгенерировать советы" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при получении персональных советов");
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
