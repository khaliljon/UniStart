using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/onboarding")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(IOnboardingService onboardingService, ILogger<OnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get onboarding status for the current user
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(OnboardingStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var status = await _onboardingService.GetStatusAsync(GetUserId());
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting onboarding status");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get available exam types with score ranges and descriptions
    /// </summary>
    [HttpGet("exam-types")]
    [ProducesResponseType(typeof(IEnumerable<ExamTypeInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamTypes()
    {
        try
        {
            var examTypes = await _onboardingService.GetExamTypesInfoAsync();
            return Ok(examTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam types");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Complete onboarding — creates study goal and generates study plan
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(OnboardingStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
    {
        try
        {
            var result = await _onboardingService.CompleteOnboardingAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing onboarding");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
