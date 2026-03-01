using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Application.Services;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly PresenceTracker _presenceTracker;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IAuthService authService, PresenceTracker presenceTracker, ILogger<UsersController> logger)
    {
        _authService = authService;
        _presenceTracker = presenceTracker;
        _logger = logger;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id)
    {
        var currentUserId = GetCurrentUserId();
        
        // Users can only access their own data (unless admin)
        if (currentUserId != id && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _authService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var currentUserId = GetCurrentUserId();
        
        if (currentUserId != id && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            var user = await _authService.UpdateUserAsync(id, dto);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get user online presence status
    /// </summary>
    [HttpGet("{id}/presence")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPresence(int id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { error = "User not found" });

        var isOnline = _presenceTracker.IsOnline(id);
        return Ok(new { isOnline, lastSeenAt = user.LastSeenAt });
    }

    /// <summary>
    /// Get presence for multiple users at once
    /// </summary>
    [HttpPost("presence/batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPresenceBatch([FromBody] int[] userIds)
    {
        var result = userIds.Select(id => new
        {
            userId = id,
            isOnline = _presenceTracker.IsOnline(id)
        });
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
