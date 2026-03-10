using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/formulas")]
[Authorize]
[ApiVersion("1.0")]
public class FormulaController : ControllerBase
{
    private readonly IFormulaService _formulaService;

    public FormulaController(IFormulaService formulaService)
    {
        _formulaService = formulaService;
    }

    /// <summary>
    /// Get formula cards filtered by exam type
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FormulaCardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFormulas([FromQuery] string[]? examTypeCodes = null)
    {
        var userId = GetCurrentUserId();
        var result = await _formulaService.GetFormulasByExamAsync(userId, examTypeCodes);
        return Ok(result);
    }

    /// <summary>
    /// Get user's bookmarked formulas
    /// </summary>
    [HttpGet("bookmarks")]
    [ProducesResponseType(typeof(IEnumerable<FormulaCardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookmarks()
    {
        var userId = GetCurrentUserId();
        var result = await _formulaService.GetBookmarkedFormulasAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Toggle bookmark on a formula card
    /// </summary>
    [HttpPost("{formulaId}/bookmark")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleBookmark(int formulaId)
    {
        var userId = GetCurrentUserId();
        var isBookmarked = await _formulaService.ToggleBookmarkAsync(userId, formulaId);
        return Ok(new { formulaId, isBookmarked });
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
