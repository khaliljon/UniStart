using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/onboarding")]
[Authorize]
[ApiVersion("1.0")]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(IOnboardingService onboardingService, ILogger<OnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id) && id > 0) return id;
        throw new UnauthorizedAccessException("Invalid user identity");
    }

    /// <summary>
    /// Get onboarding status for the current user
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(OnboardingStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _onboardingService.GetStatusAsync(GetUserId());
        return Ok(status);
    }

    /// <summary>
    /// Get available exam types with score ranges and descriptions
    /// </summary>
    [HttpGet("exam-types")]
    [ProducesResponseType(typeof(IEnumerable<ExamTypeInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamTypes()
    {
        var examTypes = await _onboardingService.GetExamTypesInfoAsync();
        return Ok(examTypes);
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
    }
}
