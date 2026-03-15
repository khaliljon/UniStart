using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/drills")]
[Authorize]
[ApiVersion("1.0")]
public class TimedDrillController : ControllerBase
{
    private readonly ITimedDrillService _drillService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly UniStartDbContext _db;

    public TimedDrillController(ITimedDrillService drillService, ISubscriptionService subscriptionService, UniStartDbContext db)
    {
        _drillService = drillService;
        _subscriptionService = subscriptionService;
        _db = db;
    }

    /// <summary>
    /// Get active drill templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _db.DrillTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                DrillType = t.DrillType.ToString(),
                t.ExamTypeCode,
                t.TopicId,
                t.QuestionCount,
                t.TimeLimitMinutes,
                t.SortOrder,
            })
            .ToListAsync();
        return Ok(templates);
    }

    /// <summary>
    /// Start a new timed drill session
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(DrillResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartDrill([FromBody] StartDrillRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _drillService.StartDrillAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Get next question for an active drill
    /// </summary>
    [HttpGet("{drillId}/next")]
    [ProducesResponseType(typeof(DrillQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetNextQuestion(int drillId)
    {
        var question = await _drillService.GetNextDrillQuestionAsync(drillId);
        if (question == null) return NoContent();
        return Ok(question);
    }

    /// <summary>
    /// Submit an answer during a drill
    /// </summary>
    [HttpPost("answer")]
    [ProducesResponseType(typeof(DrillAnswerResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitDrillAnswerRequest request)
    {
        var userId = GetCurrentUserId();
        if (!await _subscriptionService.CanAnswerQuestionAsync(userId))
            return StatusCode(429, new { error = "Daily question limit reached. Upgrade to Pro for unlimited access." });

        var result = await _drillService.SubmitDrillAnswerAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Complete/end a drill session manually
    /// </summary>
    [HttpPost("{drillId}/complete")]
    [ProducesResponseType(typeof(DrillResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteDrill(int drillId)
    {
        var userId = GetCurrentUserId();
        var result = await _drillService.CompleteDrillAsync(userId, drillId);
        return Ok(result);
    }

    /// <summary>
    /// Get personal best records for each drill type
    /// </summary>
    [HttpGet("personal-bests")]
    [ProducesResponseType(typeof(IEnumerable<PersonalBestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonalBests()
    {
        var userId = GetCurrentUserId();
        var result = await _drillService.GetPersonalBestsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get drill session history
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<DrillResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _drillService.GetDrillHistoryAsync(userId, limit);
        return Ok(result);
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
