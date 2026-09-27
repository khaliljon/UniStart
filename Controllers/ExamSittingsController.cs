using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/exam-sittings")]
public class ExamSittingsController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public ExamSittingsController(UniStartDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.ExamSittings
            .Where(e => e.IsActive)
            .OrderBy(e => e.SortOrder).ThenBy(e => e.Date)
            .Select(e => new ExamSittingDto(e.Id, e.Date, e.EndDate, e.IsActive, e.SortOrder))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminList()
    {
        var items = await _db.ExamSittings
            .OrderBy(e => e.SortOrder).ThenBy(e => e.Date)
            .Select(e => new ExamSittingDto(e.Id, e.Date, e.EndDate, e.IsActive, e.SortOrder))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SaveExamSittingDto dto)
    {
        var e = new ExamSitting { Date = dto.Date, EndDate = dto.EndDate, IsActive = dto.IsActive, SortOrder = dto.SortOrder };
        _db.ExamSittings.Add(e);
        await _db.SaveChangesAsync();
        return Ok(new ExamSittingDto(e.Id, e.Date, e.EndDate, e.IsActive, e.SortOrder));
    }

    [HttpPut("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveExamSittingDto dto)
    {
        var e = await _db.ExamSittings.FindAsync(id);
        if (e == null) return NotFound();
        e.Date = dto.Date;
        e.EndDate = dto.EndDate;
        e.IsActive = dto.IsActive;
        e.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        return Ok(new ExamSittingDto(e.Id, e.Date, e.EndDate, e.IsActive, e.SortOrder));
    }

    [HttpDelete("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.ExamSittings.FindAsync(id);
        if (e == null) return NotFound();
        _db.ExamSittings.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
