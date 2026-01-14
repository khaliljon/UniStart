using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Services.AI;

namespace UniStart.Controllers.AI;

/// <summary>
/// Контроллер для AI генерации контента
/// </summary>
[ApiController]
[Route("api/ai/generator")]
[Authorize]
public class AIGeneratorController : ControllerBase
{
    private readonly IAIContentGeneratorService _aiGenerator;
    private readonly ILogger<AIGeneratorController> _logger;

    public AIGeneratorController(
        IAIContentGeneratorService aiGenerator,
        ILogger<AIGeneratorController> logger)
    {
        _aiGenerator = aiGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Проверить доступность AI генератора
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var isAvailable = _aiGenerator.IsAvailable();
        
        return Ok(new
        {
            available = isAvailable,
            message = isAvailable 
                ? "AI генератор контента активен" 
                : "AI генератор не настроен. Добавьте API ключ в конфигурацию"
        });
    }

    /// <summary>
    /// Генерировать вопросы по теме
    /// </summary>
    [HttpPost("questions")]
    public async Task<IActionResult> GenerateQuestions([FromBody] QuestionGenerationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest(new { message = "Предмет обязателен" });

            var questions = await _aiGenerator.GenerateQuestions(
                request.Subject, 
                request.Difficulty ?? "Medium", 
                request.Count ?? 5);

            return Ok(new
            {
                subject = request.Subject,
                difficulty = request.Difficulty ?? "Medium",
                total = questions.Count,
                questions,
                isAIGenerated = _aiGenerator.IsAvailable()
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры для генерации вопросов");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI сервис недоступен для генерации вопросов");
            return StatusCode(503, new { message = "AI генератор временно недоступен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при генерации вопросов");
            return StatusCode(500, new { message = "Ошибка при генерации вопросов" });
        }
    }

    /// <summary>
    /// Получить объяснение ответа
    /// </summary>
    [HttpPost("explanation")]
    public async Task<IActionResult> GenerateExplanation([FromBody] ExplanationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.CorrectAnswer))
                return BadRequest(new { message = "Вопрос и правильный ответ обязательны" });

            var explanation = await _aiGenerator.GenerateExplanation(
                request.Question, 
                request.CorrectAnswer);

            return Ok(new
            {
                question = request.Question,
                correctAnswer = request.CorrectAnswer,
                explanation,
                isAIGenerated = _aiGenerator.IsAvailable()
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры для генерации объяснения");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI сервис недоступен для генерации объяснения");
            return StatusCode(503, new { message = "AI генератор временно недоступен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при генерации объяснения");
            return StatusCode(500, new { message = "Ошибка при генерации объяснения" });
        }
    }

    /// <summary>
    /// Получить подсказку для вопроса
    /// </summary>
    [HttpPost("hint")]
    public async Task<IActionResult> GenerateHint([FromBody] HintRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { message = "Вопрос обязателен" });

            var hint = await _aiGenerator.GenerateHint(
                request.Question, 
                request.UserAnswer ?? string.Empty);

            return Ok(new
            {
                hint,
                isAIGenerated = _aiGenerator.IsAvailable()
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры для генерации подсказки");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI сервис недоступен для генерации подсказки");
            return StatusCode(503, new { message = "AI генератор временно недоступен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при генерации подсказки");
            return StatusCode(500, new { message = "Ошибка при генерации подсказки" });
        }
    }

    /// <summary>
    /// Создать резюме учебного материала
    /// </summary>
    [HttpPost("summary")]
    public async Task<IActionResult> GenerateSummary([FromBody] SummaryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Topic) || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Тема и контент обязательны" });

            var summary = await _aiGenerator.GenerateSummary(request.Topic, request.Content);

            return Ok(new
            {
                topic = request.Topic,
                summary,
                isAIGenerated = _aiGenerator.IsAvailable()
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры для генерации резюме");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI сервис недоступен для генерации резюме");
            return StatusCode(503, new { message = "AI генератор временно недоступен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при генерации резюме");
            return StatusCode(500, new { message = "Ошибка при генерации резюме" });
        }
    }
}

// DTOs для запросов
public class QuestionGenerationRequest
{
    public string Subject { get; set; } = string.Empty;
    public string? Difficulty { get; set; }
    public int? Count { get; set; }
}

public class ExplanationRequest
{
    public string Question { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
}

public class HintRequest
{
    public string Question { get; set; } = string.Empty;
    public string? UserAnswer { get; set; }
}

public class SummaryRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
