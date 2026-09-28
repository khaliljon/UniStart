using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/specialty-tracks")]
public class SpecialtyTracksController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public SpecialtyTracksController(UniStartDbContext db) => _db = db;

    private static readonly string[] AllowedSubjects =
        { "math", "physics", "chemistry", "chineseTech", "chineseHum" };

    private static SpecialtyTrackDto ToDto(SpecialtyTrack t) => new(
        t.Id, t.Name, t.NameKz, t.NameEn,
        t.Subjects.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        t.ConditionalChinese, t.SortOrder, t.IsActive);

    private static string NormalizeSubjects(IEnumerable<string>? subjects) =>
        string.Join(',', (subjects ?? Enumerable.Empty<string>())
            .Select(x => x.Trim())
            .Where(x => AllowedSubjects.Contains(x))
            .Distinct());

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.SpecialtyTracks
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Id)
            .ToListAsync();
        return Ok(items.Select(ToDto));
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminList()
    {
        var items = await _db.SpecialtyTracks
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Id)
            .ToListAsync();
        return Ok(items.Select(ToDto));
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SaveSpecialtyTrackDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Название направления обязательно." });

        var t = new SpecialtyTrack
        {
            Name = dto.Name.Trim(),
            NameKz = dto.NameKz?.Trim(),
            NameEn = dto.NameEn?.Trim(),
            Subjects = NormalizeSubjects(dto.Subjects),
            ConditionalChinese = dto.ConditionalChinese,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
        };
        _db.SpecialtyTracks.Add(t);
        await _db.SaveChangesAsync();
        return Ok(ToDto(t));
    }

    [HttpPut("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveSpecialtyTrackDto dto)
    {
        var t = await _db.SpecialtyTracks.FindAsync(id);
        if (t == null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Название направления обязательно." });

        t.Name = dto.Name.Trim();
        t.NameKz = dto.NameKz?.Trim();
        t.NameEn = dto.NameEn?.Trim();
        t.Subjects = NormalizeSubjects(dto.Subjects);
        t.ConditionalChinese = dto.ConditionalChinese;
        t.SortOrder = dto.SortOrder;
        t.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(ToDto(t));
    }

    [HttpDelete("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.SpecialtyTracks.FindAsync(id);
        if (t == null) return NotFound();
        _db.SpecialtyTracks.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
