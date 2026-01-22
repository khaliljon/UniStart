using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniStart.DTOs;
using UniStart.Services;

namespace UniStart.Controllers.Student;

/// <summary>
/// Контроллер для управления предпочтениями пользователя и Onboarding
/// </summary>
[Authorize]
[ApiController]
[Route("api/student/[controller]")]
public class PreferencesController : ControllerBase
{
    private readonly IUserPreferencesService _preferencesService;
    private readonly ILogger<PreferencesController> _logger;

    public PreferencesController(
        IUserPreferencesService preferencesService,
        ILogger<PreferencesController> logger)
    {
        _preferencesService = preferencesService;
        _logger = logger;
    }

    /// <summary>
    /// Получить предпочтения текущего пользователя
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserPreferencesResponseDto>> GetMyPreferences()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var preferences = await _preferencesService.GetUserPreferencesAsync(userId);
            
            if (preferences == null)
                return NotFound(new { message = "Предпочтения не найдены. Пройдите Onboarding." });

            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предпочтений");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Проверить, завершен ли Onboarding
    /// </summary>
    [HttpGet("onboarding/status")]
    public async Task<ActionResult<object>> GetOnboardingStatus()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var completed = await _preferencesService.HasCompletedOnboardingAsync(userId);
            
            return Ok(new { onboardingCompleted = completed });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке статуса Onboarding");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создать или обновить предпочтения (Onboarding)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserPreferencesResponseDto>> CreateOrUpdatePreferences(
        [FromBody] UserPreferencesDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var preferences = await _preferencesService.CreateOrUpdatePreferencesAsync(userId, dto);
            
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании/обновлении предпочтений");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Завершить Onboarding
    /// </summary>
    [HttpPost("onboarding/complete")]
    public async Task<ActionResult<object>> CompleteOnboarding()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _preferencesService.CompleteOnboardingAsync(userId);
            
            if (!success)
                return NotFound(new { message = "Предпочтения не найдены" });

            return Ok(new { message = "Onboarding успешно завершен", onboardingCompleted = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при завершении Onboarding");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Пропустить Onboarding (создаст предпочтения по умолчанию)
    /// </summary>
    [HttpPost("onboarding/skip")]
    public async Task<ActionResult<object>> SkipOnboarding()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var preferences = await _preferencesService.SkipOnboardingAsync(userId);
            
            return Ok(new 
            { 
                message = "Onboarding пропущен. Вы можете настроить предпочтения позже в профиле.",
                onboardingCompleted = true,
                preferences
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при пропуске Onboarding");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить рекомендуемые предметы на основе предпочтений
    /// </summary>
    [HttpGet("recommended-subjects")]
    public async Task<ActionResult<List<SubjectDto>>> GetRecommendedSubjects()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var subjects = await _preferencesService.GetRecommendedSubjectsAsync(userId);
            
            return Ok(subjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендуемых предметов");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
