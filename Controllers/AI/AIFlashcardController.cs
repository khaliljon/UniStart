using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using UniStart.DTOs;
using UniStart.Models.Flashcards;
using UniStart.Repositories;
using UniStart.Services.AI;

namespace UniStart.Controllers.AI;

[ApiController]
[Route("api/ai/flashcards")]
[Authorize]
public class AIFlashcardController : ControllerBase
{
    private readonly IAIFlashcardGeneratorService _aiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AIFlashcardController> _logger;

    public AIFlashcardController(
        IAIFlashcardGeneratorService aiService,
        IUnitOfWork unitOfWork,
        ILogger<AIFlashcardController> logger)
    {
        _aiService = aiService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Проверить, настроен ли AI сервис
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var isConfigured = await _aiService.IsConfiguredAsync();
        var availableModels = await _aiService.GetAvailableModelsAsync();

        return Ok(new
        {
            isConfigured,
            availableModels,
            message = isConfigured 
                ? "✅ AI сервис настроен и готов к работе" 
                : "⚠️ Добавьте API ключ в appsettings.json"
        });
    }

    /// <summary>
    /// Сгенерировать flashcards из текста с помощью AI
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GenerateFlashcards([FromBody] GenerateFlashcardsRequest request)
    {
        try
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(request.SourceText))
                return BadRequest(new { message = "Исходный текст не может быть пустым" });

            if (request.Count < 1 || request.Count > 50)
                return BadRequest(new { message = "Количество должно быть от 1 до 50" });

            // Генерируем через AI
            var response = await _aiService.GenerateFlashcardsAsync(request);

            if (!response.Success)
                return BadRequest(new { message = response.ErrorMessage });

            // Если указан FlashcardSetId, проверяем доступ
            if (request.FlashcardSetId.HasValue)
            {
                var set = await _unitOfWork.Repository<FlashcardSet>()
                    .GetByIdAsync(request.FlashcardSetId.Value);

                if (set == null)
                    return NotFound(new { message = "Набор flashcards не найден" });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                if (set.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                // Добавляем flashcards в существующий набор
                foreach (var generated in response.Flashcards)
                {
                    // Перемешиваем варианты ответов (правильный не всегда первый)
                    var shuffledOptions = generated.Options.OrderBy(_ => Random.Shared.Next()).ToList();
                    
                    var flashcard = new Flashcard
                    {
                        Type = FlashcardType.SingleChoice,
                        Question = generated.Question,
                        Answer = generated.Answer,
                        OptionsJson = JsonSerializer.Serialize(shuffledOptions),
                        Explanation = generated.Explanation ?? string.Empty,
                        FlashcardSetId = set.Id
                    };
                    await _unitOfWork.Repository<Flashcard>().AddAsync(flashcard);
                }

                await _unitOfWork.SaveChangesAsync();
                response.FlashcardSetId = set.Id;

                _logger.LogInformation("Добавлено {Count} AI-генерированных flashcards в набор {SetId}", 
                    response.Flashcards.Count, set.Id);
            }
            else if (!string.IsNullOrEmpty(request.NewSetTitle))
            {
                // Создаем новый набор
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                
                var newSet = new FlashcardSet
                {
                    Title = request.NewSetTitle,
                    Description = $"Сгенерировано AI из текста ({response.Flashcards.Count} карточек)",
                    UserId = userId,
                    IsPublic = true,  // Доступно всем студентам
                    IsPublished = false,  // Черновик - можно отредактировать перед публикацией
                    CreatedAt = DateTime.UtcNow
                };
                
                await _unitOfWork.Repository<FlashcardSet>().AddAsync(newSet);
                await _unitOfWork.SaveChangesAsync();

                // Добавляем flashcards в новый набор
                foreach (var generated in response.Flashcards)
                {
                    // Перемешиваем варианты ответов (правильный не всегда первый)
                    var shuffledOptions = generated.Options.OrderBy(_ => Random.Shared.Next()).ToList();
                    
                    var flashcard = new Flashcard
                    {
                        Type = FlashcardType.SingleChoice,
                        Question = generated.Question,
                        Answer = generated.Answer,
                        OptionsJson = JsonSerializer.Serialize(shuffledOptions),
                        Explanation = generated.Explanation ?? string.Empty,
                        FlashcardSetId = newSet.Id
                    };
                    await _unitOfWork.Repository<Flashcard>().AddAsync(flashcard);
                }

                await _unitOfWork.SaveChangesAsync();
                response.FlashcardSetId = newSet.Id;

                _logger.LogInformation("Создан новый набор {SetId} с {Count} AI-генерированными flashcards", 
                    newSet.Id, response.Flashcards.Count);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации flashcards через AI");
            return StatusCode(500, new { message = "Ошибка генерации", error = ex.Message });
        }
    }

    /// <summary>
    /// Получить список доступных AI моделей
    /// </summary>
    [HttpGet("models")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAvailableModels()
    {
        var models = await _aiService.GetAvailableModelsAsync();
        
        return Ok(new
        {
            models,
            count = models.Count,
            recommended = "claude-sonnet-4.5"
        });
    }
}
