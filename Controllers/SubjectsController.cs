using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/subjects - Получить активные предметы
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Subject>>> GetSubjects()
    {
        var subjects = await _context.Subjects
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
        
        return Ok(subjects);
    }

    // GET: api/subjects/all - Получить все предметы (включая неактивные)
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Subject>>> GetAllSubjects()
    {
        var subjects = await _context.Subjects
            .OrderBy(s => s.Name)
            .ToListAsync();
        
        return Ok(subjects);
    }

    // GET: api/subjects/5 - Получить предмет по ID
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Subject>> GetSubject(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);

        if (subject == null)
        {
            return NotFound();
        }

        return Ok(subject);
    }

    // POST: api/subjects - Создать новый предмет
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Subject>> CreateSubject(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, subject);
    }

    // PUT: api/subjects/5 - Обновить предмет
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSubject(int id, Subject subject)
    {
        if (id != subject.Id)
        {
            return BadRequest();
        }

        _context.Entry(subject).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _context.Subjects.AnyAsync(e => e.Id == id);
            if (!exists)
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/subjects/5 - Мягкое удаление (IsActive = false)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null)
        {
            return NotFound();
        }

        subject.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
