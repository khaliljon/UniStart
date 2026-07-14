using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

/// <summary>Server-side cart so it follows the user across devices.</summary>
[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ApiControllerBase
{
    private readonly UniStartDbContext _db;

    public CartController(UniStartDbContext db)
    {
        _db = db;
    }

    private static CartItemDto ToDto(UserCartItem c) => new(
        c.ItemType, c.ItemCode, c.Title, c.Subjects, c.Amount, c.Currency, c.Runs,
        string.IsNullOrWhiteSpace(c.SelectedMockIds)
            ? new List<int>()
            : c.SelectedMockIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList());

    /// <summary>Current user's cart.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetCurrentUserId();
        var items = await _db.UserCartItems
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Id)
            .ToListAsync();
        return Ok(items.Select(ToDto));
    }

    /// <summary>Add or replace a cart line (unique per type + code).</summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CartItemDto dto)
    {
        var userId = GetCurrentUserId();
        var existing = await _db.UserCartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemType == dto.ItemType && c.ItemCode == dto.ItemCode);

        var selected = dto.SelectedMockIds != null ? string.Join(",", dto.SelectedMockIds) : null;
        if (existing == null)
        {
            _db.UserCartItems.Add(new UserCartItem
            {
                UserId = userId,
                ItemType = dto.ItemType.Trim(),
                ItemCode = dto.ItemCode.Trim(),
                Title = dto.Title.Trim(),
                Subjects = dto.Subjects,
                Amount = dto.Amount,
                Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "KZT" : dto.Currency.Trim(),
                Runs = dto.Runs,
                SelectedMockIds = selected,
            });
        }
        else
        {
            existing.Title = dto.Title.Trim();
            existing.Subjects = dto.Subjects;
            existing.Amount = dto.Amount;
            existing.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "KZT" : dto.Currency.Trim();
            existing.Runs = dto.Runs;
            existing.SelectedMockIds = selected;
        }
        await _db.SaveChangesAsync();
        return Ok();
    }

    /// <summary>Remove one line.</summary>
    [HttpDelete("item")]
    public async Task<IActionResult> RemoveItem([FromQuery] string itemType, [FromQuery] string itemCode)
    {
        var userId = GetCurrentUserId();
        var item = await _db.UserCartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemType == itemType && c.ItemCode == itemCode);
        if (item != null)
        {
            _db.UserCartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    /// <summary>Empty the cart.</summary>
    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var userId = GetCurrentUserId();
        var items = _db.UserCartItems.Where(c => c.UserId == userId);
        _db.UserCartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record CartItemDto(
    string ItemType,
    string ItemCode,
    string Title,
    string? Subjects,
    decimal Amount,
    string Currency,
    int? Runs,
    List<int>? SelectedMockIds);
