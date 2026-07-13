using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

/// <summary>
/// Global pricing settings. Prices are stored as admin-editable key/value
/// settings so the frontend never hard-codes them.
/// </summary>
[ApiController]
[Route("api")]
public class PricingController : ControllerBase
{
    private readonly UniStartDbContext _db;

    // Defaults used until an admin sets values.
    private const decimal DefaultMockPrice = 990m;
    private const decimal DefaultMaterialPrice = 6990m;
    private const string DefaultCurrency = "KZT";

    private const string KeyMockPrice = "MockPrice";
    private const string KeyMaterialPrice = "MaterialPrice";
    private const string KeyCurrency = "Currency";

    public PricingController(UniStartDbContext db)
    {
        _db = db;
    }

    /// <summary>Current pricing. Public — used by landing and cart.</summary>
    [HttpGet("pricing")]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var settings = await _db.AppSettings
            .Where(s => s.Key == KeyMockPrice || s.Key == KeyMaterialPrice || s.Key == KeyCurrency)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        var mockPrice = TryDecimal(settings, KeyMockPrice, DefaultMockPrice);
        var materialPrice = TryDecimal(settings, KeyMaterialPrice, DefaultMaterialPrice);
        var currency = settings.TryGetValue(KeyCurrency, out var c) && !string.IsNullOrWhiteSpace(c)
            ? c
            : DefaultCurrency;

        return Ok(new PricingDto(mockPrice, materialPrice, currency));
    }

    /// <summary>Update pricing (admin only).</summary>
    [HttpPut("admin/pricing")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdatePricingDto dto)
    {
        if (dto.MockPrice < 0 || dto.MaterialPrice < 0)
            return BadRequest(new { error = "Price cannot be negative." });

        var currency = string.IsNullOrWhiteSpace(dto.Currency) ? DefaultCurrency : dto.Currency.Trim();

        await UpsertAsync(KeyMockPrice, dto.MockPrice.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await UpsertAsync(KeyMaterialPrice, dto.MaterialPrice.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await UpsertAsync(KeyCurrency, currency);
        await _db.SaveChangesAsync();

        return Ok(new PricingDto(dto.MockPrice, dto.MaterialPrice, currency));
    }

    private async Task UpsertAsync(string key, string value)
    {
        var existing = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (existing == null)
        {
            _db.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            existing.Value = value;
        }
    }

    private static decimal TryDecimal(IReadOnlyDictionary<string, string> settings, string key, decimal fallback)
        => settings.TryGetValue(key, out var raw)
           && decimal.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val)
            ? val
            : fallback;
}
