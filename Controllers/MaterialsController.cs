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
            m.TitleKz,
            m.TitleEn,
            m.DescriptionKz,
            m.DescriptionEn,
            m.Price,
        }));
    }

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

        var itemCode = id.ToString();
        var hasFullAccess = await _db.Users.AnyAsync(u => u.Id == userId && u.HasFullAccess);
        var purchased = hasFullAccess || await _db.Purchases.AnyAsync(p =>
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
            m.TitleKz, m.TitleEn, m.DescriptionKz, m.DescriptionEn,
            m.PdfUrl, m.Price, m.IsActive, m.CreatedAt, m.UpdatedAt
        }));
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SaveStudyMaterialDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PdfUrl))
            return BadRequest(new { error = "Загрузите PDF файл — без него сохранить нельзя." });

        var item = new StudyMaterial
        {
            SubjectKey = dto.SubjectKey.Trim(),
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            TitleKz = string.IsNullOrWhiteSpace(dto.TitleKz) ? null : dto.TitleKz.Trim(),
            TitleEn = string.IsNullOrWhiteSpace(dto.TitleEn) ? null : dto.TitleEn.Trim(),
            DescriptionKz = string.IsNullOrWhiteSpace(dto.DescriptionKz) ? null : dto.DescriptionKz.Trim(),
            DescriptionEn = string.IsNullOrWhiteSpace(dto.DescriptionEn) ? null : dto.DescriptionEn.Trim(),
            PdfUrl = dto.PdfUrl?.Trim(),
            Price = dto.Price,
            IsActive = dto.IsActive,
        };
        _db.StudyMaterials.Add(item);
        await _db.SaveChangesAsync();
        return Ok(new { item.Id, item.SubjectKey, item.Title, item.Price, item.IsActive });
    }

    [HttpPut("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveStudyMaterialDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PdfUrl))
            return BadRequest(new { error = "Загрузите PDF файл — без него сохранить нельзя." });

        var item = await _db.StudyMaterials.FindAsync(id);
        if (item == null) return NotFound();

        item.SubjectKey = dto.SubjectKey.Trim();
        item.Title = dto.Title.Trim();
        item.Description = dto.Description?.Trim();
        item.TitleKz = string.IsNullOrWhiteSpace(dto.TitleKz) ? null : dto.TitleKz.Trim();
        item.TitleEn = string.IsNullOrWhiteSpace(dto.TitleEn) ? null : dto.TitleEn.Trim();
        item.DescriptionKz = string.IsNullOrWhiteSpace(dto.DescriptionKz) ? null : dto.DescriptionKz.Trim();
        item.DescriptionEn = string.IsNullOrWhiteSpace(dto.DescriptionEn) ? null : dto.DescriptionEn.Trim();
        if (dto.PdfUrl != null) item.PdfUrl = dto.PdfUrl.Trim();
        item.Price = dto.Price;
        item.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return Ok(new { item.Id, item.SubjectKey, item.Title, item.Price, item.IsActive });
    }

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

    [HttpPost("admin/presign-pdf")]
    [Authorize(Roles = "Admin")]
    public IActionResult PresignPdf()
    {
        try
        {
            var target = _upload.CreatePdfUploadTarget();
            return Ok(new { uploadUrl = target.UploadUrl, publicUrl = target.PublicUrl });
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

public record SaveStudyMaterialDto(
    string SubjectKey,
    string Title,
    string? Description,
    string? PdfUrl,
    decimal Price,
    bool IsActive,
    string? TitleKz = null,
    string? TitleEn = null,
    string? DescriptionKz = null,
    string? DescriptionEn = null
);
