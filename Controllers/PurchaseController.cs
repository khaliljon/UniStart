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
    /// Admin sales monitor: all purchases with buyer info and revenue totals.
    /// Optional filters by status, item type and date range.
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminList([FromQuery] string? status, [FromQuery] string? itemType,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var rows = await FilteredRowsAsync(status, itemType, from, to);
        var currency = rows.FirstOrDefault()?.Currency ?? "KZT";
        var revenue = rows.Where(r => r.Status == "Paid").Sum(r => r.Amount);
        var net = rows.Where(r => r.Status == "Paid").Sum(r => r.NetAmount);
        return Ok(new AdminSalesDto(rows.Count, revenue, currency, rows, net));
    }

    /// <summary>Export the (filtered) sales list as CSV — Admin only.</summary>
    [HttpGet("admin/export.csv")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportCsv([FromQuery] string? status, [FromQuery] string? itemType,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var rows = await FilteredRowsAsync(status, itemType, from, to);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Date,User,Email,Type,Code,Title,Subjects,Amount,Currency,Status,Gross,Tax,Fee,FeeCurrency,Net,Total");
        foreach (var r in rows)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            sb.AppendLine(string.Join(",",
                Csv(r.PurchasedAt.ToString("o")),
                Csv(r.UserName), Csv(r.UserEmail), Csv(r.ItemType), Csv(r.ItemCode),
                Csv(r.Title), Csv(r.Subjects ?? ""),
                Csv(r.Amount.ToString(inv)),
                Csv(r.Currency), Csv(r.Status),
                Csv(r.GrossAmount.ToString(inv)), Csv(r.TaxAmount.ToString(inv)),
                Csv(r.PlatformFeeAmount.ToString(inv)), Csv(r.PlatformFeeCurrency ?? ""),
                Csv(r.NetAmount.ToString(inv)), Csv(r.TotalAmount.ToString(inv))));
        }
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"sales-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private async Task<List<AdminPurchaseDto>> FilteredRowsAsync(string? status, string? itemType, DateTime? from, DateTime? to)
    {
        var query = _db.Purchases.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(itemType))
            query = query.Where(p => p.ItemType == itemType);
        if (from.HasValue)
            query = query.Where(p => p.PurchasedAt >= from.Value.ToUniversalTime());
        if (to.HasValue)
            query = query.Where(p => p.PurchasedAt < to.Value.ToUniversalTime().AddDays(1));

        return await query
            .OrderByDescending(p => p.PurchasedAt)
            .Select(p => new AdminPurchaseDto(
                p.Id, p.UserId, p.User.Name, p.User.Email, p.ItemType, p.ItemCode,
                p.Title, p.Subjects, p.Amount, p.Currency, p.Status, p.PurchasedAt,
                p.GrossAmount, p.TaxAmount, p.PlatformFeeAmount, p.PlatformFeeCurrency, p.NetAmount, p.TotalAmount))
            .ToListAsync();
    }

    private static string Csv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
