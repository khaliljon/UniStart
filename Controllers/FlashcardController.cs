using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/flashcards")]
[Authorize]
[ApiVersion("1.0")]
public class FlashcardController : ControllerBase
{
    private readonly IFlashcardService _flashcardService;

    public FlashcardController(IFlashcardService flashcardService)
    {
        _flashcardService = flashcardService;
    }

    /// <summary>
    /// Get all accessible decks with stats
    /// </summary>
    [HttpGet("decks")]
    [ProducesResponseType(typeof(IEnumerable<FlashcardDeckDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDecks([FromQuery] string[]? examTypeCodes = null)
    {
        var userId = GetCurrentUserId();
        var result = await _flashcardService.GetDecksAsync(userId, examTypeCodes);
        return Ok(result);
    }

    /// <summary>
    /// Get due cards for review from a specific deck
    /// </summary>
    [HttpGet("decks/{deckId}/due")]
    [ProducesResponseType(typeof(IEnumerable<FlashcardReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueCards(int deckId, [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _flashcardService.GetDueCardsAsync(userId, deckId, limit);
        return Ok(result);
    }

    /// <summary>
    /// Submit a review result for a flashcard (SM-2 algorithm)
    /// </summary>
    [HttpPost("review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReviewCard([FromBody] ReviewFlashcardRequest request)
    {
        var userId = GetCurrentUserId();
        await _flashcardService.ReviewCardAsync(userId, request);
        return NoContent();
    }

    /// <summary>
    /// Create a new personal deck
    /// </summary>
    [HttpPost("decks")]
    [ProducesResponseType(typeof(FlashcardDeckDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDeck([FromBody] CreateDeckRequest request)
    {
        var userId = GetCurrentUserId();
        var deck = await _flashcardService.CreateDeckAsync(userId, request);
        return CreatedAtAction(nameof(GetDecks), new { }, deck);
    }

    /// <summary>
    /// Add a card to a deck
    /// </summary>
    [HttpPost("cards")]
    [ProducesResponseType(typeof(FlashcardDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddCard([FromBody] CreateFlashcardRequest request)
    {
        var userId = GetCurrentUserId();
        var card = await _flashcardService.AddCardAsync(userId, request);
        return Created("", card);
    }

    /// <summary>
    /// Delete a personal deck
    /// </summary>
    [HttpDelete("decks/{deckId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDeck(int deckId)
    {
        var userId = GetCurrentUserId();
        await _flashcardService.DeleteDeckAsync(userId, deckId);
        return NoContent();
    }

    /// <summary>
    /// Get total due cards count across all decks
    /// </summary>
    [HttpGet("due-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueCount()
    {
        var userId = GetCurrentUserId();
        var count = await _flashcardService.GetTotalDueCountAsync(userId);
        return Ok(new { dueCount = count });
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
