using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/study-plan")]
[Authorize]
[ApiVersion("1.0")]
[FeatureGate("StudyPlan")]
public class StudyPlanController : ControllerBase
{
    private readonly IStudyPlanService _service;
    private readonly ILogger<StudyPlanController> _logger;

    public StudyPlanController(IStudyPlanService service, ILogger<StudyPlanController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("goals")]
    [ProducesResponseType(typeof(StudyGoalDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGoal([FromBody] CreateStudyGoalDto dto)
    {
        var userId = GetCurrentUserId();
        var goal = await _service.CreateGoalAsync(userId, dto);
        return CreatedAtAction(nameof(GetActiveGoal), null, goal);
    }

    [HttpGet("goals/active")]
    [ProducesResponseType(typeof(StudyGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveGoal()
    {
        var userId = GetCurrentUserId();
        var goal = await _service.GetActiveGoalAsync(userId);
        return Ok(goal);
    }

    [HttpPut("goals/{goalId}")]
    [ProducesResponseType(typeof(StudyGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateGoal(int goalId, [FromBody] UpdateStudyGoalDto dto)
    {
        var userId = GetCurrentUserId();
        var goal = await _service.UpdateGoalAsync(userId, goalId, dto);
        if (goal == null) return NotFound();
        return Ok(goal);
    }

    [HttpDelete("goals/{goalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGoal(int goalId)
    {
        var userId = GetCurrentUserId();
        var deleted = await _service.DeleteGoalAsync(userId, goalId);
        if (!deleted) return NotFound();
        return NoContent();
    }

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

    [HttpGet("active")]
    [ProducesResponseType(typeof(StudyPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePlan()
    {
        var userId = GetCurrentUserId();
        var plan = await _service.GetActivePlanAsync(userId);
        return Ok(plan);
    }

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

    [HttpGet("today")]
    [ProducesResponseType(typeof(TodayPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayPlan()
    {
        var userId = GetCurrentUserId();
        var today = await _service.GetTodayPlanAsync(userId);
        return Ok(today);
    }

    [HttpPost("entries/{entryId}/complete")]
    [ProducesResponseType(typeof(StudyPlanEntryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteEntry(int entryId, [FromBody] CompleteEntryDto dto)
    {
        var userId = GetCurrentUserId();
        var entry = await _service.CompleteEntryAsync(userId, entryId, dto);
        if (entry == null) return NotFound();
        return Ok(entry);
    }

    [HttpPost("auto-complete-today")]
    [ProducesResponseType(typeof(TodayPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AutoCompleteToday()
    {
        var userId = GetCurrentUserId();
        var todayPlan = await _service.AutoCompleteTodayAsync(userId);
        return Ok(todayPlan);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(PlanStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanStats()
    {
        var userId = GetCurrentUserId();
        var stats = await _service.GetPlanStatsAsync(userId);
        return Ok(stats);
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
