using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
[ApiVersion("1.0")]
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
    /// Get user's skill analytics (legacy)
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
    /// Get full analytics dashboard data (charts, streaks, breakdowns)
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();
        var dashboard = await _analyticsService.GetDashboardAsync(userId);
        return Ok(dashboard);
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

    /// <summary>
    /// Get skill level history over time
    /// </summary>
    [HttpGet("skill-history")]
    [ProducesResponseType(typeof(IEnumerable<SkillHistoryPointDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkillHistory([FromQuery] int days = 30)
    {
        var userId = GetCurrentUserId();
        var history = await _analyticsService.GetSkillHistoryAsync(userId, days);
        return Ok(history);
    }

    /// <summary>
    /// Get daily activity heatmap data
    /// </summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(IEnumerable<DailyActivityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivity([FromQuery] int days = 90)
    {
        var userId = GetCurrentUserId();
        var activity = await _analyticsService.GetActivityHeatmapAsync(userId, days);
        return Ok(activity);
    }

    /// <summary>
    /// Get accuracy breakdown by difficulty level
    /// </summary>
    [HttpGet("difficulty")]
    [ProducesResponseType(typeof(IEnumerable<DifficultyStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDifficultyBreakdown()
    {
        var userId = GetCurrentUserId();
        var breakdown = await _analyticsService.GetDifficultyBreakdownAsync(userId);
        return Ok(breakdown);
    }

    // ─── Test Sessions ───────────────────────────────────────────

    /// <summary>
    /// Start a new test session
    /// </summary>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(TestSessionSummaryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var userId = GetCurrentUserId();
        var session = await _analyticsService.StartSessionAsync(userId, request.ExamTypeCode, request.Mode);
        return CreatedAtAction(nameof(GetSessionDetail), new { sessionId = session.Id }, session);
    }

    /// <summary>
    /// Complete (finish) a test session
    /// </summary>
    [HttpPost("sessions/{sessionId}/complete")]
    [ProducesResponseType(typeof(TestSessionSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteSession(int sessionId)
    {
        var session = await _analyticsService.CompleteSessionAsync(sessionId);
        return Ok(session);
    }

    /// <summary>
    /// Get test session history with pagination
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<TestSessionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var sessions = await _analyticsService.GetSessionsAsync(userId, page, pageSize);
        return Ok(sessions);
    }

    /// <summary>
    /// Get detailed test session with all answers
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    [ProducesResponseType(typeof(TestSessionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionDetail(int sessionId)
    {
        var userId = GetCurrentUserId();
        var session = await _analyticsService.GetSessionDetailAsync(userId, sessionId);
        if (session == null) return NotFound();
        return Ok(session);
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

public record StartSessionRequest(string ExamTypeCode, string Mode = "practice");
