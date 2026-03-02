using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/strategies")]
[Authorize]
[ApiVersion("1.0")]
public class StrategyController : ControllerBase
{
    private readonly IStrategyService _strategyService;

    public StrategyController(IStrategyService strategyService)
    {
        _strategyService = strategyService;
    }

    /// <summary>
    /// Get strategy guides for a specific exam
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StrategyGuideSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGuides([FromQuery] string examTypeCode)
    {
        var userId = GetCurrentUserId();
        var result = await _strategyService.GetGuidesByExamAsync(userId, examTypeCode);
        return Ok(result);
    }

    /// <summary>
    /// Get full strategy guide content
    /// </summary>
    [HttpGet("{guideId}")]
    [ProducesResponseType(typeof(StrategyGuideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGuide(int guideId)
    {
        var userId = GetCurrentUserId();
        var result = await _strategyService.GetGuideAsync(userId, guideId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Mark a strategy guide as read
    /// </summary>
    [HttpPost("{guideId}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkRead(int guideId)
    {
        var userId = GetCurrentUserId();
        await _strategyService.MarkReadAsync(userId, guideId);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("Invalid user identity");
        return userId;
    }
}
