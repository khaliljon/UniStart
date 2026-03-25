using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

/// <summary>
/// School admin dashboard — for school owners to manage their students and tutors
/// </summary>
[ApiController]
[Route("api/school-admin")]
[Authorize(Roles = "Tutor,Admin,SchoolAdmin")]
[ApiVersion("1.0")]
public class SchoolAdminController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public SchoolAdminController(UniStartDbContext db)
    {
        _db = db;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<TutorSchool?> GetOwnedSchool()
    {
        var userId = GetUserId();
        return await _db.TutorSchools.FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.IsActive);
    }

    /// <summary>Dashboard summary</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var students = await _db.Users
            .Where(u => u.SchoolId == school.Id && !u.IsDeleted && u.Role == UserRole.Student)
            .ToListAsync();

        var tutorCount = await _db.TutorProfiles.CountAsync(t => t.SchoolId == school.Id);

        var activeStudents = students.Count(s => s.LastSeenAt > sevenDaysAgo);

        // Average accuracy from recent test sessions of school students
        var studentIds = students.Select(s => s.Id).ToHashSet();
        var avgAccuracy = await _db.TestSessions
            .Where(ts => studentIds.Contains(ts.UserId) && ts.CompletedAt != null && ts.TotalQuestions > 0)
            .OrderByDescending(ts => ts.CompletedAt)
            .Take(500)
            .AverageAsync(ts => (double?)ts.CorrectCount * 100.0 / ts.TotalQuestions) ?? 0;

        var recentStudents = students
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .Select(s => new SchoolStudentDto(
                s.Id, s.Name, s.Email, s.SubscriptionTier.ToString(),
                s.CreatedAt, s.LastSeenAt, s.LinkedTutorId, null))
            .ToList();

        return Ok(new SchoolDashboardDto(
            school.Id, school.Name, students.Count, tutorCount,
            activeStudents, Math.Round(avgAccuracy, 1), recentStudents));
    }

    /// <summary>All students in the school</summary>
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        var query = _db.Users
            .Where(u => u.SchoolId == school.Id && !u.IsDeleted && u.Role == UserRole.Student);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new SchoolStudentDto(
                u.Id, u.Name, u.Email, u.SubscriptionTier.ToString(),
                u.CreatedAt, u.LastSeenAt, u.LinkedTutorId, null))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>All tutors in the school</summary>
    [HttpGet("tutors")]
    public async Task<IActionResult> GetTutors()
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        var tutors = await _db.TutorProfiles
            .Include(t => t.User)
            .Where(t => t.SchoolId == school.Id)
            .Select(t => new SchoolTutorDto(
                t.UserId, t.User.Name, t.User.Email, t.Headline,
                t.Specializations.Split(',', System.StringSplitOptions.RemoveEmptyEntries),
                t.IsVerified, t.IsAvailable, t.TotalStudents,
                t.AverageRating, t.CreatedAt))
            .ToListAsync();

        return Ok(tutors);
    }

    /// <summary>Student analytics for school owner</summary>
    [HttpGet("students/{userId:int}/analytics")]
    public async Task<IActionResult> GetStudentAnalytics(int userId)
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        var student = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.SchoolId == school.Id && !u.IsDeleted);
        if (student == null) return NotFound(new { error = "Student not in your school" });

        var sessions = await _db.TestSessions
            .Where(ts => ts.UserId == userId && ts.CompletedAt != null && ts.TotalQuestions > 0)
            .OrderByDescending(ts => ts.CompletedAt)
            .Take(50)
            .Select(ts => new {
                ts.Id, ts.ExamTypeCode, ts.TotalQuestions, ts.CorrectCount,
                Accuracy = (double)ts.CorrectCount * 100.0 / ts.TotalQuestions,
                ts.CompletedAt
            })
            .ToListAsync();

        var skills = await _db.UserSkillProfiles
            .Where(sp => sp.UserId == userId)
            .Include(sp => sp.Skill)
            .Select(sp => new { sp.Skill.Name, sp.Level, sp.Theta, sp.ThetaSE })
            .ToListAsync();

        return Ok(new
        {
            student = new { student.Id, student.Name, student.Email, student.CreatedAt, student.LastSeenAt },
            recentSessions = sessions,
            skills
        });
    }
}
