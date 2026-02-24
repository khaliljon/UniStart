using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _svc;

    public RecommendationController(IRecommendationService svc)
    {
        _svc = svc;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Daily briefing: streak, recommendations, milestones, yesterday summary</summary>
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyBriefing()
    {
        var result = await _svc.GetDailyBriefingAsync(GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>After-session recommendations based on errors and performance</summary>
    [HttpGet("after-session/{sessionId:int}")]
    public async Task<IActionResult> GetAfterSession(int sessionId)
    {
        var result = await _svc.GetAfterSessionRecommendationsAsync(GetCurrentUserId(), sessionId);
        return Ok(result);
    }

    /// <summary>Current streak info</summary>
    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak()
    {
        var result = await _svc.GetStreakAsync(GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>All milestones for user</summary>
    [HttpGet("milestones")]
    public async Task<IActionResult> GetMilestones()
    {
        var result = await _svc.GetMilestonesAsync(GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>Check and award new milestones</summary>
    [HttpPost("check-milestones")]
    public async Task<IActionResult> CheckMilestones()
    {
        var result = await _svc.CheckAndAwardMilestonesAsync(GetCurrentUserId());
        return Ok(result);
    }
}
