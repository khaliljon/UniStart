using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Services.AI;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;

namespace UniStart.Controllers.AI;

/// <summary>
/// Контроллер для рекомендаций университетов
/// </summary>
[ApiController]
[Route("api/ai/university-recommendations")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IUniversityRecommendationService _recommendationService;
    private readonly ILogger<RecommendationsController> _logger;
    private readonly ApplicationDbContext _context;

    public RecommendationsController(
        IUniversityRecommendationService recommendationService,
        ILogger<RecommendationsController> logger,
        ApplicationDbContext context)
    {
        _recommendationService = recommendationService;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Получить персональные рекомендации университетов
    /// </summary>
    /// <param name="limit">Количество рекомендаций (по умолчанию 10)</param>
    /// <param name="forceRefresh">Принудительно пересчитать рекомендации</param>
    [HttpGet]
    public async Task<IActionResult> GetUniversityRecommendations(
        [FromQuery] int limit = 10,
        [FromQuery] bool forceRefresh = false)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (limit < 1 || limit > 50)
                return BadRequest(new { message = "limit должен быть от 1 до 50" });

            var recommendations = await _recommendationService.GetRecommendations(userId, limit, forceRefresh);

            var result = recommendations.Select(r => new
            {
                id = r.Id,
                university = new
                {
                    id = r.University.Id,
                    name = r.University.Name,
                    nameEn = r.University.NameEn,
                    city = r.University.City,
                    country = r.University.Country?.Name,
                    description = r.University.Description,
                    website = r.University.Website,
                    type = r.University.Type.ToString(),
                    tuitionFee = r.University.TuitionFee,
                    minScore = r.University.MinScore
                },
                matchScore = r.MatchScore,
                admissionProbability = r.AdmissionProbability,
                reasons = JsonSerializer.Deserialize<List<string>>(r.ReasonsJson),
                rank = r.Rank,
                isViewed = r.IsViewed,
                userRating = r.UserRating,
                createdAt = r.CreatedAt
            });

            return Ok(new
            {
                total = recommendations.Count,
                recommendations = result
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры запроса рекомендаций");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения операции получения рекомендаций");
            return StatusCode(500, new { message = "Не удалось получить рекомендации" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при получении рекомендаций университетов");
            return StatusCode(500, new { message = "Ошибка при получении рекомендаций" });
        }
    }

    /// <summary>
    /// Получить профиль пользователя
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _recommendationService.BuildUserProfile(userId);
            
            if (profile == null)
                return NotFound(new { message = "Профиль не найден или недостаточно данных" });

            return Ok(profile);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан");
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения операции получения профиля");
            return StatusCode(500, new { message = "Не удалось построить профиль" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при получении профиля пользователя");
            return StatusCode(500, new { message = "Ошибка при получении профиля" });
        }
    }

    /// <summary>
    /// Обновить предпочтения пользователя
    /// </summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _recommendationService.UpdateUserPreferences(
                userId,
                dto.PreferredCity,
                dto.MaxBudget,
                dto.CareerGoal);

            if (success)
            {
                return Ok(new { message = "Предпочтения обновлены", shouldRefreshRecommendations = true });
            }
            else
            {
                return NotFound(new { message = "Пользователь не найден" });
            }
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "UserId не указан при обновлении предпочтений");
            return Unauthorized();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при обновлении предпочтений");
            return StatusCode(500, new { message = "Не удалось сохранить предпочтения" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при обновлении предпочтений");
            return StatusCode(500, new { message = "Ошибка при обновлении предпочтений" });
        }
    }

    /// <summary>
    /// Отметить рекомендацию как просмотренную
    /// </summary>
    [HttpPost("{recommendationId}/view")]
    public async Task<IActionResult> MarkAsViewed(int recommendationId)
    {
        try
        {
            await _recommendationService.MarkAsViewed(recommendationId);
            return Ok(new { message = "Рекомендация отмечена как просмотренная" });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при отметке рекомендации {Id}", recommendationId);
            return StatusCode(500, new { message = "Не удалось обновить статус" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при отметке рекомендации {Id} как просмотренной", recommendationId);
            return StatusCode(500, new { message = "Ошибка при обновлении статуса" });
        }
    }

    /// <summary>
    /// Оценить рекомендацию (1-5 звезд)
    /// </summary>
    [HttpPost("{recommendationId}/rate")]
    public async Task<IActionResult> RateRecommendation(int recommendationId, [FromBody] RateRecommendationDto dto)
    {
        try
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Оценка должна быть от 1 до 5" });

            await _recommendationService.RateRecommendation(recommendationId, dto.Rating);
            return Ok(new { message = "Спасибо за оценку!" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректная оценка для рекомендации {Id}", recommendationId);
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при сохранении оценки рекомендации {Id}", recommendationId);
            return StatusCode(500, new { message = "Не удалось сохранить оценку" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при оценке рекомендации {Id}", recommendationId);
            return StatusCode(500, new { message = "Ошибка при сохранении оценки" });
        }
    }

    /// <summary>
    /// Получить детальное объяснение рекомендации
    /// </summary>
    [HttpGet("{universityId}/explanation")]
    public async Task<IActionResult> GetRecommendationExplanation(int universityId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recommendation = await _context.UniversityRecommendations
                .Include(r => r.University)
                    .ThenInclude(u => u.Country)
                .FirstOrDefaultAsync(r => r.University.Id == universityId && r.UserId == userId);

            if (recommendation == null)
                return NotFound(new { message = "Рекомендация не найдена" });

            var reasons = JsonSerializer.Deserialize<List<string>>(recommendation.ReasonsJson) ?? new List<string>();
            
            var explanation = $"Университет {recommendation.University.Name} рекомендован вам на основе следующих факторов:\n\n";
            explanation += $"🎯 Совпадение: {recommendation.MatchScore}%\n\n";
            
            if (recommendation.AdmissionProbability > 0)
            {
                explanation += $"📊 Вероятность поступления: {recommendation.AdmissionProbability}%\n\n";
            }

            explanation += "Причины рекомендации:\n";
            foreach (var reason in reasons)
            {
                explanation += $"• {reason}\n";
            }

            explanation += $"\n📍 Расположение: {recommendation.University.City}, {recommendation.University.Country?.Name}\n";
            
            if (recommendation.University.TuitionFee.HasValue)
            {
                explanation += $"💰 Стоимость обучения: ${recommendation.University.TuitionFee:N0} в год\n";
            }

            if (recommendation.University.MinScore.HasValue)
            {
                explanation += $"📝 Минимальный балл: {recommendation.University.MinScore}\n";
            }

            explanation += $"\n🏛️ Тип учреждения: {recommendation.University.Type}\n";
            
            if (!string.IsNullOrEmpty(recommendation.University.Description))
            {
                explanation += $"\nО университете:\n{recommendation.University.Description}";
            }

            return Ok(new { explanation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении объяснения рекомендации университета {Id}", universityId);
            return StatusCode(500, new { message = "Ошибка при получении объяснения" });
        }
    }
}

public record UpdatePreferencesDto(
    string? PreferredCity,
    decimal? MaxBudget,
    string? CareerGoal
);

public record RateRecommendationDto(
    int Rating
);
