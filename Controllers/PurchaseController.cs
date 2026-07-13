using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize]
public class PurchaseController : ApiControllerBase
{
    private readonly UniStartDbContext _db;

    public PurchaseController(UniStartDbContext db)
    {
        _db = db;
    }

    private static PurchaseDto ToDto(Purchase p) => new(
        p.Id, p.ItemType, p.ItemCode, p.Title, p.Subjects,
        p.Amount, p.Currency, p.Status, p.PurchasedAt);

    /// <summary>Current user's purchase history, newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetCurrentUserId();
        var items = await _db.Purchases
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync();
        return Ok(items.Select(ToDto));
    }

    /// <summary>
    /// Confirm an order. Payment is currently a stub (no real gateway);
    /// this records the purchase so it appears in "Мои покупки".
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
    {
        var userId = GetCurrentUserId();
        var purchase = new Purchase
        {
            UserId = userId,
            ItemType = dto.ItemType.Trim(),
            ItemCode = dto.ItemCode.Trim(),
            Title = dto.Title.Trim(),
            Subjects = string.IsNullOrWhiteSpace(dto.Subjects) ? null : dto.Subjects.Trim(),
            Amount = dto.Amount,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "KZT" : dto.Currency.Trim(),
            Status = "Paid",
            PurchasedAt = DateTime.UtcNow,
        };
        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();
        return Ok(ToDto(purchase));
    }

    /// <summary>
    /// Admin sales monitor: all purchases with buyer info and revenue totals.
    /// Optional filters by status and item type.
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminList([FromQuery] string? status, [FromQuery] string? itemType)
    {
        var query = _db.Purchases.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(itemType))
            query = query.Where(p => p.ItemType == itemType);

        var rows = await query
            .OrderByDescending(p => p.PurchasedAt)
            .Select(p => new AdminPurchaseDto(
                p.Id,
                p.UserId,
                p.User.Name,
                p.User.Email,
                p.ItemType,
                p.ItemCode,
                p.Title,
                p.Subjects,
                p.Amount,
                p.Currency,
                p.Status,
                p.PurchasedAt))
            .ToListAsync();

        var currency = rows.FirstOrDefault()?.Currency ?? "KZT";
        var revenue = rows.Where(r => r.Status == "Paid").Sum(r => r.Amount);

        return Ok(new AdminSalesDto(rows.Count, revenue, currency, rows));
    }
}
