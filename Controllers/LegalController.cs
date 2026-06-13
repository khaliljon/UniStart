using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

/// <summary>
/// Public read access to legal documents + admin editing.
/// </summary>
[ApiController]
[Route("api/legal")]
public class LegalController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public LegalController(UniStartDbContext db)
    {
        _db = db;
    }

    // ── Public ──────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var docs = await _db.LegalDocuments
            .OrderBy(d => d.Slug)
            .Select(d => new LegalDocumentDto(d.Slug, d.Title, d.LastUpdatedLabel, d.Content, d.UpdatedAt))
            .ToListAsync();
        return Ok(docs);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var doc = await _db.LegalDocuments
            .Where(d => d.Slug == slug)
            .Select(d => new LegalDocumentDto(d.Slug, d.Title, d.LastUpdatedLabel, d.Content, d.UpdatedAt))
            .FirstOrDefaultAsync();

        if (doc == null) return NotFound();
        return Ok(doc);
    }

    // ── Admin ───────────────────────────────────────

    [HttpPut("{slug}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateLegalDocumentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest(new { error = "Title and content are required." });
        }

        var doc = await _db.LegalDocuments.FirstOrDefaultAsync(d => d.Slug == slug);
        if (doc == null) return NotFound();

        doc.Title = dto.Title.Trim();
        doc.LastUpdatedLabel = dto.LastUpdatedLabel?.Trim() ?? string.Empty;
        doc.Content = dto.Content;
        await _db.SaveChangesAsync();

        return Ok(new LegalDocumentDto(doc.Slug, doc.Title, doc.LastUpdatedLabel, doc.Content, doc.UpdatedAt));
    }
}
