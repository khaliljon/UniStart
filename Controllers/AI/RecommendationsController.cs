using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Services.AI;
using System.Security.Claims;
using System.Text.Json;

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

    public RecommendationsController(
        IUniversityRecommendationService recommendationService,
        ILogger<RecommendationsController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    /// <summary>
    /// Получить персональные рекомендации университетов
    /// </summary>
    /// <param name="topN">Количество рекомендаций (по умолчанию 10)</param>
    /// <param name="forceRefresh">Принудительно пересчитать рекомендации</param>
    [HttpGet("universities")]
    public async Task<IActionResult> GetUniversityRecommendations(
        [FromQuery] int topN = 10,
        [FromQuery] bool forceRefresh = false)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (topN < 1 || topN > 50)
                return BadRequest(new { message = "topN должно быть от 1 до 50" });

            var recommendations = await _recommendationService.GetRecommendations(userId, topN, forceRefresh);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендаций университетов");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении профиля пользователя");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении предпочтений");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отметке рекомендации {Id}", recommendationId);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при оценке рекомендации {Id}", recommendationId);
            return StatusCode(500, new { message = "Ошибка при сохранении оценки" });
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
