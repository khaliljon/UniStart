using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _svc;

    public AdminController(IAdminService svc)
    {
        _svc = svc;
    }

    /// <summary>List questions with optional filters</summary>
    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] string? examTypeCode = null,
        [FromQuery] string? topic = null,
        [FromQuery] string? difficulty = null)
    {
        var result = await _svc.GetQuestionsAsync(examTypeCode, topic, difficulty);
        return Ok(result);
    }

    /// <summary>Get question details by ID</summary>
    [HttpGet("questions/{id:int}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        var result = await _svc.GetQuestionByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Create a new question</summary>
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        try
        {
            var result = await _svc.CreateQuestionAsync(dto);
            return CreatedAtAction(nameof(GetQuestion), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an existing question</summary>
    [HttpPut("questions/{id:int}")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto dto)
    {
        var result = await _svc.UpdateQuestionAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Delete a question</summary>
    [HttpDelete("questions/{id:int}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var ok = await _svc.DeleteQuestionAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    /// <summary>Bulk import questions (JSON array)</summary>
    [HttpPost("questions/import")]
    public async Task<IActionResult> BulkImport([FromBody] BulkImportDto dto)
    {
        var result = await _svc.BulkImportAsync(dto);
        return Ok(result);
    }

    /// <summary>Question database stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _svc.GetStatsAsync();
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════
    //  USERS
    // ═══════════════════════════════════════════════════════

    /// <summary>List all users with optional filters</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role = null,
        [FromQuery] string? search = null)
    {
        var result = await _svc.GetUsersAsync(role, search);
        return Ok(result);
    }

    /// <summary>Get user details by ID</summary>
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _svc.GetUserByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Update user (role, subscription, etc.)</summary>
    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        try
        {
            var result = await _svc.UpdateUserAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Delete a user and all related data</summary>
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var ok = await _svc.DeleteUserAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>User statistics summary</summary>
    [HttpGet("users/stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var result = await _svc.GetUserStatsAsync();
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════
    //  DASHBOARD & TOPICS
    // ═══════════════════════════════════════════════════════

    /// <summary>Admin dashboard with overview stats</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _svc.GetDashboardAsync();
        return Ok(result);
    }

    /// <summary>List all topics with question counts</summary>
    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics()
    {
        var result = await _svc.GetTopicsAsync();
        return Ok(result);
    }
}
