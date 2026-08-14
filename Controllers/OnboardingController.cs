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

    [HttpGet("status")]
    [ProducesResponseType(typeof(OnboardingStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _onboardingService.GetStatusAsync(GetUserId());
        return Ok(status);
    }

    [HttpGet("exam-types")]
    [ProducesResponseType(typeof(IEnumerable<ExamTypeInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamTypes()
    {
        var lang = Request.Headers["Accept-Language"].FirstOrDefault() ?? "ru";
        if (lang != "kz" && lang != "en") lang = "ru";
        var examTypes = await _onboardingService.GetExamTypesInfoAsync(lang);
        return Ok(examTypes);
    }

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
