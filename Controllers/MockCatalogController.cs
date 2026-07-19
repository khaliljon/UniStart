using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

/// <summary>
/// Run-based mock storefront: public catalog, checkout (stub grant — Polar wires
/// in later), and admin CRUD for price tiers and packages.
/// </summary>
[ApiController]
[Route("api/mock-catalog")]
public class MockCatalogController : ControllerBase
{
    private readonly IEntitlementService _entitlements;
    private readonly UniStartDbContext _db;

    public MockCatalogController(IEntitlementService entitlements, UniStartDbContext db)
    {
        _entitlements = entitlements;
        _db = db;
    }

    private int CurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) && id > 0 ? id : 0;
    }

    /// <summary>Public catalog. If authenticated, includes the user's run balances.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCatalog()
        => Ok(await _entitlements.GetCatalogAsync(CurrentUserId()));

    /// <summary>Price a cart without paying.</summary>
    [HttpPost("quote")]
    [Authorize]
    public async Task<IActionResult> Quote([FromBody] RunCheckoutDto dto)
    {
        try { return Ok(await _entitlements.QuoteAsync(dto.Lines)); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // ── Admin: price tiers ─────────────────────────────────

    [HttpGet("admin/tiers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminTiers()
    {
        var tiers = await _db.MockPriceTiers
            .OrderBy(t => t.MockExamId).ThenBy(t => t.Runs)
            .Select(t => new { t.Id, t.MockExamId, t.Runs, t.Price, t.Currency, t.IsActive })
            .ToListAsync();
        return Ok(tiers);
    }

    [HttpPost("admin/tiers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTier([FromBody] SaveTierDto dto)
    {
        var tier = new MockPriceTier
        {
            MockExamId = dto.MockExamId,
            Runs = dto.Runs,
            Price = dto.Price,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "KZT" : dto.Currency.Trim(),
            IsActive = dto.IsActive,
        };
        _db.MockPriceTiers.Add(tier);
        await _db.SaveChangesAsync();
        return Ok(new { tier.Id });
    }

    [HttpPut("admin/tiers/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTier(int id, [FromBody] SaveTierDto dto)
    {
        var tier = await _db.MockPriceTiers.FindAsync(id);
        if (tier == null) return NotFound();
        tier.MockExamId = dto.MockExamId;
        tier.Runs = dto.Runs;
        tier.Price = dto.Price;
        tier.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "KZT" : dto.Currency.Trim();
        tier.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { tier.Id });
    }

    [HttpDelete("admin/tiers/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTier(int id)
    {
        var tier = await _db.MockPriceTiers.FindAsync(id);
        if (tier == null) return NotFound();
        _db.MockPriceTiers.Remove(tier);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── Admin: packages ────────────────────────────────────

    [HttpGet("admin/packages")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminPackages()
    {
        var packages = await _db.MockPackages
            .OrderBy(p => p.SortOrder)
            .Select(p => new { p.Id, p.Key, p.Name, p.NameKz, p.NameEn, p.PickCount, p.RunsEach, p.Price, p.Currency, p.SortOrder, p.IsActive })
            .ToListAsync();
        return Ok(packages);
    }

    [HttpPost("admin/packages")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePackage([FromBody] SavePackageDto dto)
    {
        var pkg = new MockPackage
        {
            Key = dto.Key.Trim(),
            Name = dto.Name.Trim(),
            NameKz = string.IsNullOrWhiteSpace(dto.NameKz) ? null : dto.NameKz.Trim(),
            NameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim(),
            PickCount = dto.PickCount,
            RunsEach = dto.RunsEach,
            Price = dto.Price,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "KZT" : dto.Currency.Trim(),
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
        };
        _db.MockPackages.Add(pkg);
        await _db.SaveChangesAsync();
        return Ok(new { pkg.Id });
    }

    [HttpPut("admin/packages/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePackage(int id, [FromBody] SavePackageDto dto)
    {
        var pkg = await _db.MockPackages.FindAsync(id);
        if (pkg == null) return NotFound();
        pkg.Key = dto.Key.Trim();
        pkg.Name = dto.Name.Trim();
        pkg.NameKz = string.IsNullOrWhiteSpace(dto.NameKz) ? null : dto.NameKz.Trim();
        pkg.NameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim();
        pkg.PickCount = dto.PickCount;
        pkg.RunsEach = dto.RunsEach;
        pkg.Price = dto.Price;
        pkg.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "KZT" : dto.Currency.Trim();
        pkg.SortOrder = dto.SortOrder;
        pkg.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { pkg.Id });
    }

    [HttpDelete("admin/packages/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var pkg = await _db.MockPackages.FindAsync(id);
        if (pkg == null) return NotFound();
        _db.MockPackages.Remove(pkg);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class SaveTierDto
{
    public int MockExamId { get; set; }
    public int Runs { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "KZT";
    public bool IsActive { get; set; } = true;
}

public class SavePackageDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameKz { get; set; }
    public string? NameEn { get; set; }
    public int PickCount { get; set; }
    public int RunsEach { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "KZT";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
