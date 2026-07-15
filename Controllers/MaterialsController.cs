using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/materials")]
public class MaterialsController : ControllerBase
{
    private readonly UniStartDbContext _db;
    private readonly IImageUploadService _upload;

    public MaterialsController(UniStartDbContext db, IImageUploadService upload)
    {
        _db = db;
        _upload = upload;
    }

    // ═══════════════════════════════════════════════════════════════
    //  PUBLIC / USER ENDPOINTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>List all active study materials (public — shown on landing and dashboard).</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.StudyMaterials
            .Where(m => m.IsActive)
            .OrderBy(m => m.SubjectKey)
            .AsNoTracking()
            .ToListAsync();

        return Ok(items.Select(m => new
        {
            m.Id,
            m.SubjectKey,
            m.Title,
            m.Description,
            m.Price,
        }));
    }

    /// <summary>
    /// Returns the PDF download URL for a purchased material.
    /// Requires the user to be authenticated and to own a Purchase for this specific material.
    /// </summary>
    [HttpGet("{id:int}/download")]
    [Authorize]
    public async Task<IActionResult> Download(int id)
    {
        var userId = GetUserId();

        var material = await _db.StudyMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

        if (material == null)
            return NotFound(new { error = "Материал не найден." });

        // Check that the user has purchased THIS specific material (per-material ownership).
        var itemCode = id.ToString();
        var purchased = await _db.Purchases.AnyAsync(p =>
            p.UserId == userId &&
            p.ItemType == "book" &&
            p.ItemCode == itemCode &&
            p.Status == "Paid");

        if (!purchased)
            return StatusCode(403, new { error = "Доступ к PDF возможен только после покупки." });

        if (string.IsNullOrEmpty(material.PdfUrl))
            return NotFound(new { error = "PDF файл ещё не загружен." });

        return Ok(new { pdfUrl = material.PdfUrl });
    }

    // ═══════════════════════════════════════════════════════════════
    //  ADMIN ENDPOINTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>List all materials (including inactive) — Admin only.</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminList()
    {
        var items = await _db.StudyMaterials
            .OrderBy(m => m.SubjectKey)
            .AsNoTracking()
            .ToListAsync();

        return Ok(items.Select(m => new
        {
            m.Id, m.SubjectKey, m.Title, m.Description,
            m.PdfUrl, m.Price, m.IsActive, m.CreatedAt, m.UpdatedAt
        }));
    }

    /// <summary>Create a new study material — Admin only.</summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SaveStudyMaterialDto dto)
    {
        var item = new StudyMaterial
        {
            SubjectKey = dto.SubjectKey.Trim(),
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            PdfUrl = dto.PdfUrl?.Trim(),
            Price = dto.Price,
            IsActive = dto.IsActive,
        };
        _db.StudyMaterials.Add(item);
        await _db.SaveChangesAsync();
        return Ok(new { item.Id, item.SubjectKey, item.Title, item.Price, item.IsActive });
    }

    /// <summary>Update an existing study material — Admin only.</summary>
    [HttpPut("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveStudyMaterialDto dto)
    {
        var item = await _db.StudyMaterials.FindAsync(id);
        if (item == null) return NotFound();

        item.SubjectKey = dto.SubjectKey.Trim();
        item.Title = dto.Title.Trim();
        item.Description = dto.Description?.Trim();
        if (dto.PdfUrl != null) item.PdfUrl = dto.PdfUrl.Trim();
        item.Price = dto.Price;
        item.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return Ok(new { item.Id, item.SubjectKey, item.Title, item.Price, item.IsActive });
    }

    /// <summary>Delete a study material — Admin only.</summary>
    [HttpDelete("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.StudyMaterials.FindAsync(id);
        if (item == null) return NotFound();

        _db.StudyMaterials.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    /// <summary>Upload a PDF file to R2 and return its URL — Admin only.</summary>
    [HttpPost("admin/upload-pdf")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(536_870_912)] // 512 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 536_870_912)]
    public async Task<IActionResult> UploadPdf(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Файл не выбран." });

        var allowedTypes = new[] { "application/pdf", "application/octet-stream" };
        if (!allowedTypes.Contains(file.ContentType) && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Допускается только PDF-файл." });

        try
        {
            var url = await _upload.UploadPdfAsync(file, ct);
            return Ok(new { url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim == null || !int.TryParse(claim.Value, out var id) || id <= 0)
            throw new UnauthorizedAccessException("Invalid user identity");
        return id;
    }
}

// ─── DTOs ────────────────────────────────────────────────
public record SaveStudyMaterialDto(
    string SubjectKey,
    string Title,
    string? Description,
    string? PdfUrl,
    decimal Price,
    bool IsActive
);
