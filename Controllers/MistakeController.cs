using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/mistakes")]
[Authorize]
[ApiVersion("1.0")]
public class MistakeController : ControllerBase
{
    private readonly IMistakeService _mistakeService;

    public MistakeController(IMistakeService mistakeService)
    {
        _mistakeService = mistakeService;
    }

    /// <summary>
    /// Get paginated list of mistakes with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MistakeEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMistakes(
        [FromQuery] string? examTypeCode = null,
        [FromQuery] int? topicId = null,
        [FromQuery] string? errorType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _mistakeService.GetMistakesAsync(userId, examTypeCode, topicId, errorType, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Set error type classification on a mistake
    /// </summary>
    [HttpPost("error-type")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetErrorType([FromBody] SetErrorTypeRequest request)
    {
        var userId = GetCurrentUserId();
        await _mistakeService.SetErrorTypeAsync(userId, request);
        return NoContent();
    }

    /// <summary>
    /// Set or update a note on a mistake
    /// </summary>
    [HttpPost("note")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetNote([FromBody] SetMistakeNoteRequest request)
    {
        var userId = GetCurrentUserId();
        await _mistakeService.SetNoteAsync(userId, request);
        return NoContent();
    }

    /// <summary>
    /// Get mistake analysis with error patterns and topic breakdown
    /// </summary>
    [HttpGet("analysis")]
    [ProducesResponseType(typeof(MistakeAnalysisDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalysis([FromQuery] string? examTypeCode = null)
    {
        var userId = GetCurrentUserId();
        var result = await _mistakeService.GetAnalysisAsync(userId, examTypeCode);
        return Ok(result);
    }

    /// <summary>
    /// Get total mistake count
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMistakeCount()
    {
        var userId = GetCurrentUserId();
        var count = await _mistakeService.GetMistakeCountAsync(userId);
        return Ok(new { count });
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
