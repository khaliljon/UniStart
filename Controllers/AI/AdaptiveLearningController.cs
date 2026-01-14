using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Services.AI;
using System.Security.Claims;

namespace UniStart.Controllers.AI;

/// <summary>
/// Контроллер для адаптивного обучения с ML предсказаниями
/// </summary>
[ApiController]
[Route("api/ai/adaptive-learning")]
[Authorize]
public class AdaptiveLearningController : ControllerBase
{
    private readonly IMLPredictionService _mlService;
    private readonly ILogger<AdaptiveLearningController> _logger;

    public AdaptiveLearningController(
        IMLPredictionService mlService,
        ILogger<AdaptiveLearningController> logger)
    {
        _mlService = mlService;
        _logger = logger;
    }

    /// <summary>
    /// Получить ML-предсказание оптимального времени следующего повторения карточки
    /// </summary>
    /// <param name="flashcardId">ID карточки</param>
    /// <returns>Предсказание с рекомендацией и уверенностью модели</returns>
    [HttpGet("next-review/{flashcardId}")]
    public async Task<IActionResult> GetNextReviewPrediction(int flashcardId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var prediction = await _mlService.PredictNextReviewTime(userId, flashcardId);
            
            if (prediction == null)
                return NotFound(new { message = "Прогресс по карточке не найден" });

            return Ok(new
            {
                flashcardId = prediction.FlashcardId,
                optimalReviewHours = prediction.OptimalReviewHours,
                recommendedReviewDate = prediction.RecommendedReviewDate,
                confidence = prediction.Confidence,
                reason = prediction.Reason,
                isMLPrediction = _mlService.IsModelTrained()
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры для предсказания flashcard {FlashcardId}", flashcardId);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка ML модели при предсказании для карточки {FlashcardId}", flashcardId);
            return StatusCode(500, new { message = "Модель не готова" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при получении предсказания для карточки {FlashcardId}", flashcardId);
            return StatusCode(500, new { message = "Ошибка при получении предсказания" });
        }
    }

    /// <summary>
    /// Получить персональный план обучения на сегодня
    /// </summary>
    /// <returns>Список карточек к повторению с ML рекомендациями</returns>
    [HttpGet("study-plan")]
    public async Task<IActionResult> GetStudyPlan()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studyPlan = await _mlService.GenerateStudyPlan(userId);
            
            return Ok(new
            {
                totalCards = studyPlan.Count,
                isMLActive = _mlService.IsModelTrained(),
                recommendations = studyPlan.Select(item => new
                {
                    flashcardId = item.FlashcardId,
                    optimalReviewHours = item.OptimalReviewHours,
                    recommendedReviewDate = item.RecommendedReviewDate,
                    confidence = item.Confidence,
                    reason = item.Reason,
                    priority = item.OptimalReviewHours <= 1 ? "urgent" : 
                               item.OptimalReviewHours <= 24 ? "high" : "normal"
                })
            });
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан для генерации плана обучения");
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка ML модели при генерации плана");
            return StatusCode(500, new { message = "Модель не готова для генерации плана" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при генерации плана обучения");
            return StatusCode(500, new { message = "Ошибка при генерации плана" });
        }
    }

    /// <summary>
    /// Переобучить ML модель
    /// </summary>
    /// <returns>Результат переобучения</returns>
    [HttpPost("retrain")]
    public async Task<IActionResult> RetrainModel()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Запущено переобучение ML модели пользователем {UserId}", userId);
            
            var success = await _mlService.RetrainModel();
            
            if (success)
            {
                return Ok(new { message = "ML модель успешно переобучена", modelTrained = true });
            }
            else
            {
                return BadRequest(new { message = "Недостаточно данных для переобучения модели (требуется минимум 100 примеров)" });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка конфигурации ML модели при переобучении");
            return StatusCode(500, new { message = "Ошибка конфигурации модели" });
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Ошибка ввода-вывода при сохранении ML модели");
            return StatusCode(500, new { message = "Не удалось сохранить модель" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при переобучении ML модели");
            return StatusCode(500, new { message = "Ошибка при переобучении модели" });
        }
    }

    /// <summary>
    /// Проверить статус ML модели
    /// </summary>
    [HttpGet("model-status")]
    public IActionResult GetModelStatus()
    {
        var isTrained = _mlService.IsModelTrained();
        
        return Ok(new
        {
            isModelTrained = isTrained,
            status = isTrained ? "active" : "fallback_to_sm2",
            message = isTrained 
                ? "ML модель активна и делает предсказания" 
                : "ML модель не обучена, используется статический алгоритм SM-2"
        });
    }
}
