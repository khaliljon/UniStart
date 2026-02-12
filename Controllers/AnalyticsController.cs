using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get user's skill analytics
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SkillAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics()
    {
        var userId = GetCurrentUserId();
        var analytics = await _analyticsService.GetUserAnalyticsAsync(userId);
        return Ok(analytics);
    }

    /// <summary>
    /// Get user's skills breakdown
    /// </summary>
    [HttpGet("skills")]
    [ProducesResponseType(typeof(IEnumerable<UserSkillProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills()
    {
        var userId = GetCurrentUserId();
        var skills = await _analyticsService.GetUserSkillsAsync(userId);
        return Ok(skills);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirst("sub");
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
