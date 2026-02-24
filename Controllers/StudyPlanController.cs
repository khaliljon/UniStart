using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/study-plan")]
[Authorize]
public class StudyPlanController : ControllerBase
{
    private readonly IStudyPlanService _service;
    private readonly ILogger<StudyPlanController> _logger;

    public StudyPlanController(IStudyPlanService service, ILogger<StudyPlanController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ─── Goals ───────────────────────────────────────────────

    /// <summary>
    /// Create a new study goal (deactivates previous goals)
    /// </summary>
    [HttpPost("goals")]
    [ProducesResponseType(typeof(StudyGoalDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGoal([FromBody] CreateStudyGoalDto dto)
    {
        var userId = GetCurrentUserId();
        var goal = await _service.CreateGoalAsync(userId, dto);
        return CreatedAtAction(nameof(GetActiveGoal), null, goal);
    }

    /// <summary>
    /// Get the user's active study goal
    /// </summary>
    [HttpGet("goals/active")]
    [ProducesResponseType(typeof(StudyGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveGoal()
    {
        var userId = GetCurrentUserId();
        var goal = await _service.GetActiveGoalAsync(userId);
        if (goal == null) return NotFound(new { message = "No active goal" });
        return Ok(goal);
    }

    /// <summary>
    /// Update a study goal
    /// </summary>
    [HttpPut("goals/{goalId}")]
    [ProducesResponseType(typeof(StudyGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateGoal(int goalId, [FromBody] UpdateStudyGoalDto dto)
    {
        var userId = GetCurrentUserId();
        var goal = await _service.UpdateGoalAsync(userId, goalId, dto);
        if (goal == null) return NotFound();
        return Ok(goal);
    }

    /// <summary>
    /// Delete (deactivate) a study goal
    /// </summary>
    [HttpDelete("goals/{goalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGoal(int goalId)
    {
        var userId = GetCurrentUserId();
        var deleted = await _service.DeleteGoalAsync(userId, goalId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ─── Plan ────────────────────────────────────────────────

    /// <summary>
    /// Generate a study plan for the specified goal
    /// </summary>
    [HttpPost("generate/{goalId}")]
    [ProducesResponseType(typeof(StudyPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GeneratePlan(int goalId)
    {
        var userId = GetCurrentUserId();
        try
        {
            var plan = await _service.GeneratePlanAsync(userId, goalId);
            return Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get the user's active study plan with all entries
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(StudyPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePlan()
    {
        var userId = GetCurrentUserId();
        var plan = await _service.GetActivePlanAsync(userId);
        if (plan == null) return NotFound(new { message = "No active plan" });
        return Ok(plan);
    }

    /// <summary>
    /// Regenerate the plan based on current progress (dynamic adaptation)
    /// </summary>
    [HttpPost("regenerate")]
    [ProducesResponseType(typeof(StudyPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RegeneratePlan()
    {
        var userId = GetCurrentUserId();
        try
        {
            var plan = await _service.RegeneratePlanAsync(userId);
            return Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ─── Today ───────────────────────────────────────────────

    /// <summary>
    /// Get today's study tasks
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(TodayPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayPlan()
    {
        var userId = GetCurrentUserId();
        var today = await _service.GetTodayPlanAsync(userId);
        return Ok(today);
    }

    // ─── Entry Completion ────────────────────────────────────

    /// <summary>
    /// Mark a plan entry as completed with results
    /// </summary>
    [HttpPost("entries/{entryId}/complete")]
    [ProducesResponseType(typeof(StudyPlanEntryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteEntry(int entryId, [FromBody] CompleteEntryDto dto)
    {
        var userId = GetCurrentUserId();
        var entry = await _service.CompleteEntryAsync(userId, entryId, dto);
        if (entry == null) return NotFound();
        return Ok(entry);
    }

    // ─── Stats ───────────────────────────────────────────────

    /// <summary>
    /// Get plan adherence and performance statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(PlanStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanStats()
    {
        var userId = GetCurrentUserId();
        var stats = await _service.GetPlanStatsAsync(userId);
        return Ok(stats);
    }

    // ─── Helpers ─────────────────────────────────────────────

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("sub");
        var userId = int.Parse(userIdClaim?.Value ?? "0");
        return userId > 0 ? userId : 1;
    }
}
