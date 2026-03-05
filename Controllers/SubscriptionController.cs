using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
[ApiVersion("1.0")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id) && id > 0) return id;
        throw new UnauthorizedAccessException("Invalid user identity");
    }

    /// <summary>Full subscription status including tier, limits, daily usage</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _subscriptionService.GetStatusAsync(GetUserId());
        return Ok(status);
    }

    /// <summary>Daily usage only (lightweight endpoint for frequent polling)</summary>
    [HttpGet("daily-usage")]
    public async Task<IActionResult> GetDailyUsage()
    {
        var usage = await _subscriptionService.GetDailyUsageAsync(GetUserId());
        return Ok(usage);
    }

    /// <summary>Check access to a specific feature</summary>
    [HttpGet("access/{feature}")]
    public async Task<IActionResult> CheckAccess(string feature)
    {
        var hasAccess = await _subscriptionService.HasAccessAsync(GetUserId(), feature);
        return Ok(new { feature, hasAccess });
    }

    /// <summary>Upgrade or downgrade plan (stub for future payment integration)</summary>
    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] UpgradeRequestDto dto)
    {
        try
        {
            var result = await _subscriptionService.UpgradeAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>Get tier limits configuration (no auth needed for display)</summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public IActionResult GetPlans()
    {
        return Ok(new
        {
            free = _subscriptionService.GetLimits("Free"),
            pro = _subscriptionService.GetLimits("Pro"),
        });
    }
}
