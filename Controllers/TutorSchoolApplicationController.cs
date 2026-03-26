using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

/// <summary>
/// Tutor ↔ School application endpoints:
/// - Tutors submit applications to join a school
/// - SchoolAdmin reviews (approve/reject)
/// - Tutors can check their application status
/// </summary>
[ApiController]
[Route("api/tutor-school-applications")]
[Authorize]
[ApiVersion("1.0")]
public class TutorSchoolApplicationController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public TutorSchoolApplicationController(UniStartDbContext db)
    {
        _db = db;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ─── Tutor Side ─────────────────────────────────────

    /// <summary>Submit application to join a school (for tutors)</summary>
    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> SubmitApplication([FromBody] SubmitTutorSchoolAppDto dto)
    {
        var userId = GetUserId();

        // Validate school exists
        var school = await _db.TutorSchools.FindAsync(dto.SchoolId);
        if (school == null || !school.IsActive)
            return BadRequest(new { error = "School not found" });

        // Check for duplicate pending application
        var existing = await _db.TutorSchoolApplications
            .AnyAsync(a => a.UserId == userId && a.SchoolId == dto.SchoolId
                && a.Status == TutorSchoolApplicationStatus.Pending);
        if (existing)
            return Conflict(new { error = "You already have a pending application for this school" });

        // Check if tutor already belongs to this school
        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile?.SchoolId == dto.SchoolId)
            return Conflict(new { error = "You already belong to this school" });

        var app = new TutorSchoolApplication
        {
            UserId = userId,
            SchoolId = dto.SchoolId,
            Message = dto.Message?.Trim(),
        };
        _db.TutorSchoolApplications.Add(app);
        await _db.SaveChangesAsync();

        return Ok(new { id = app.Id, status = app.Status.ToString() });
    }

    /// <summary>Get my applications (for tutors)</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GetMyApplications()
    {
        var userId = GetUserId();
        var apps = await _db.TutorSchoolApplications
            .Where(a => a.UserId == userId)
            .Include(a => a.School)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.SchoolId,
                SchoolName = a.School.Name,
                SchoolSlug = a.School.Slug,
                Status = a.Status.ToString(),
                a.Message,
                a.CreatedAt,
                a.ReviewedAt
            })
            .ToListAsync();

        return Ok(apps);
    }

    // ─── SchoolAdmin Side ───────────────────────────────

    /// <summary>Get tutor applications for my school (for school admin)</summary>
    [HttpGet("school")]
    [Authorize(Roles = "SchoolAdmin,Admin")]
    public async Task<IActionResult> GetSchoolApplications([FromQuery] string? status)
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        var query = _db.TutorSchoolApplications
            .Where(a => a.SchoolId == school.Id)
            .Include(a => a.User);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TutorSchoolApplicationStatus>(status, true, out var s))
            query = query.Where(a => a.Status == s).Include(a => a.User);

        var apps = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.UserId,
                UserName = a.User.Name,
                UserEmail = a.User.Email,
                Status = a.Status.ToString(),
                a.Message,
                a.CreatedAt,
                a.ReviewedAt
            })
            .ToListAsync();

        return Ok(apps);
    }

    /// <summary>Approve or reject a tutor application (for school admin)</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "SchoolAdmin,Admin")]
    public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateTutorAppStatusDto dto)
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        var app = await _db.TutorSchoolApplications
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.SchoolId == school.Id);
        if (app == null) return NotFound(new { error = "Application not found" });

        if (!Enum.TryParse<TutorSchoolApplicationStatus>(dto.Status, true, out var newStatus)
            || newStatus == TutorSchoolApplicationStatus.Pending)
            return BadRequest(new { error = "Invalid status. Use 'Approved' or 'Rejected'" });

        app.Status = newStatus;
        app.ReviewedAt = DateTime.UtcNow;
        app.ReviewedByUserId = GetUserId();

        // On approval: bind tutor to school (verification done separately by school admin)
        if (newStatus == TutorSchoolApplicationStatus.Approved)
        {
            var user = app.User;
            user.SchoolId = school.Id;

            var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile != null)
            {
                profile.SchoolId = school.Id;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { id = app.Id, status = newStatus.ToString() });
    }

    // ─── Helpers ────────────────────────────────────────

    private async Task<TutorSchool?> GetOwnedSchool()
    {
        var userId = GetUserId();
        // SchoolAdmin: school they own; Admin: any school (via query param not used here)
        return await _db.TutorSchools.FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.IsActive);
    }
}

// ─── DTOs ────────────────────────────────────────────
public record SubmitTutorSchoolAppDto(int SchoolId, string? Message);
public record UpdateTutorAppStatusDto(string Status);
