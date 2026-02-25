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
}
