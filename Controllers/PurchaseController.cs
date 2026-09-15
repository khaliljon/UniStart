using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
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

    private static PurchaseDto ToDto(Purchase p, string? orderCode = null) => new(
        p.Id, p.ItemType, p.ItemCode, p.Title, p.Subjects,
        p.Amount, p.Currency, p.Status, p.PurchasedAt, p.PolarOrderId, p.CheckoutRef,
        p.PaymentProvider, p.ExternalPaymentId, orderCode);

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetCurrentUserId();
        var items = await _db.Purchases
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync();

        var orderIds = items.Where(p => p.PaymentOrderId != null).Select(p => p.PaymentOrderId!.Value).Distinct().ToList();
        var codeByOrderId = orderIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.PaymentOrders
                .Where(o => orderIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.OrderCode);

        return Ok(items.Select(p => ToDto(p,
            p.PaymentOrderId != null && codeByOrderId.TryGetValue(p.PaymentOrderId.Value, out var c) ? c : null)));
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminList([FromQuery] string? status, [FromQuery] string? itemType,
        [FromQuery] string? provider, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var rows = await FilteredRowsAsync(status, itemType, provider, from, to);
        var paid = rows.Where(r => r.Status == "Paid").ToList();
        var currency = paid.FirstOrDefault()?.Currency ?? rows.FirstOrDefault()?.Currency ?? "KZT";
        var revenue = paid.Sum(r => r.Amount);

        // One order = one PaymentOrder (OrderCode); legacy Polar groups by PolarOrderId/CheckoutRef.
        // A single checkout that produced several Purchase rows counts once.
        var paidOrders = paid.Select(OrderKey).Distinct().Count();

        var fees = new List<SalesFeeDto>();
        // Polar: real stored platform fee, grouped by its own currency (often USD).
        foreach (var g in paid.Where(r => EffectiveProvider(r) == "Polar" && r.PlatformFeeAmount > 0)
                              .GroupBy(r => (r.PlatformFeeCurrency ?? currency).ToUpperInvariant()))
            fees.Add(new SalesFeeDto("Polar", g.Key, Math.Round(g.Sum(r => r.PlatformFeeAmount), 2)));
        // Kaspi: provider-specific 0.95% of the accepted amount (not stored), per currency.
        foreach (var g in paid.Where(r => EffectiveProvider(r) == "Kaspi")
                              .GroupBy(r => r.Currency.ToUpperInvariant()))
            fees.Add(new SalesFeeDto("Kaspi", g.Key, Math.Round(g.Sum(r => r.Amount) * 0.0095m, 2)));

        return Ok(new AdminSalesDto(revenue, currency, paidOrders, fees, rows));
    }

    private const decimal KaspiFeeRate = 0.0095m;

    private static string EffectiveProvider(AdminPurchaseDto r) =>
        r.PaymentProvider ?? (r.PolarOrderId != null ? "Polar" : "—");

    private static string OrderKey(AdminPurchaseDto r) =>
        r.OrderCode != null ? "oc:" + r.OrderCode
        : r.PolarOrderId != null ? "po:" + r.PolarOrderId
        : r.CheckoutRef != null ? "cr:" + r.CheckoutRef
        : "id:" + r.Id;

    [HttpGet("admin/export.csv")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportCsv([FromQuery] string? status, [FromQuery] string? itemType,
        [FromQuery] string? provider, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var rows = await FilteredRowsAsync(status, itemType, provider, from, to);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Date,User,Email,Type,Code,Title,Subjects,Amount,Currency,Status,Gross,Tax,Fee,FeeCurrency,Net,Total,Provider,ExternalPaymentId,OrderCode");
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
                Csv(r.NetAmount.ToString(inv)), Csv(r.TotalAmount.ToString(inv)),
                Csv(EffectiveProvider(r)), Csv(r.ExternalPaymentId ?? ""), Csv(r.OrderCode ?? "")));
        }
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"sales-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private async Task<List<AdminPurchaseDto>> FilteredRowsAsync(string? status, string? itemType, string? provider, DateTime? from, DateTime? to)
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
        if (!string.IsNullOrWhiteSpace(provider))
        {
            if (provider.Equals("polar", StringComparison.OrdinalIgnoreCase))
                query = query.Where(p => p.PaymentProvider == "Polar" || (p.PaymentProvider == null && p.PolarOrderId != null));
            else if (provider.Equals("kaspi", StringComparison.OrdinalIgnoreCase))
                query = query.Where(p => p.PaymentProvider == "Kaspi");
        }

        var rows = await query.OrderByDescending(p => p.PurchasedAt).ToListAsync();

        var orderIds = rows.Where(p => p.PaymentOrderId != null).Select(p => p.PaymentOrderId!.Value).Distinct().ToList();
        var codeByOrderId = orderIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.PaymentOrders.Where(o => orderIds.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.OrderCode);

        return rows.Select(p => new AdminPurchaseDto(
            p.Id, p.UserId, p.User.Name, p.User.Email, p.ItemType, p.ItemCode,
            p.Title, p.Subjects, p.Amount, p.Currency, p.Status, p.PurchasedAt,
            p.GrossAmount, p.TaxAmount, p.PlatformFeeAmount, p.PlatformFeeCurrency, p.NetAmount, p.TotalAmount,
            p.PaymentProvider, p.ExternalPaymentId,
            p.PaymentOrderId != null && codeByOrderId.TryGetValue(p.PaymentOrderId.Value, out var c) ? c : null,
            p.PolarOrderId, p.CheckoutRef)).ToList();
    }

    private static string Csv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
