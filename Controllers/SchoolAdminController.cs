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
[Authorize(Roles = "Tutor,SchoolTutor,Admin,SchoolAdmin")]
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
        var school = await _db.TutorSchools.FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.IsActive);
        if (school == null)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.SchoolId != null)
                school = await _db.TutorSchools.FirstOrDefaultAsync(s => s.Id == user.SchoolId && s.IsActive);
        }
        return school;
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

        var tutorCount = await _db.TutorProfiles.CountAsync(t => t.SchoolId == school.Id && (t.User.Role == UserRole.Tutor || t.User.Role == UserRole.SchoolTutor));

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
            activeStudents, Math.Round(avgAccuracy, 1), recentStudents, school.IsApproved));
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
            .Where(t => t.SchoolId == school.Id && (t.User.Role == UserRole.Tutor || t.User.Role == UserRole.SchoolTutor))
            .Select(t => new SchoolTutorDto(
                t.UserId, t.User.Name, t.User.Email, t.Headline,
                t.Specializations.Split(',', System.StringSplitOptions.RemoveEmptyEntries),
                t.IsVerified, t.IsAvailable, t.TotalStudents,
                t.AverageRating, t.CreatedAt, t.VerificationRequestedAt))
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

    /// <summary>Assignments created by school tutors</summary>
    [HttpGet("tutor-content")]
    public async Task<IActionResult> GetTutorContent(
        [FromQuery] int? tutorId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        var tutorIds = await _db.TutorProfiles
            .Where(t => t.SchoolId == school.Id && (t.User.Role == UserRole.Tutor || t.User.Role == UserRole.SchoolTutor))
            .Select(t => t.UserId)
            .ToListAsync();

        if (tutorId.HasValue && !tutorIds.Contains(tutorId.Value))
            return BadRequest(new { error = "Tutor not in your school" });

        var filterIds = tutorId.HasValue ? new List<int> { tutorId.Value } : tutorIds;

        // Assignments
        var assignmentsQuery = _db.Set<Assignment>()
            .Include(a => a.TutorUser)
            .Include(a => a.Questions)
            .Include(a => a.Students)
            .Where(a => filterIds.Contains(a.TutorUserId));

        var totalAssignments = await assignmentsQuery.CountAsync();
        var assignments = await assignmentsQuery
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.Id, a.Title, a.Description, a.Deadline, a.IsActive, a.CreatedAt,
                tutorName = a.TutorUser.Name,
                tutorUserId = a.TutorUserId,
                questionCount = a.Questions.Count,
                studentCount = a.Students.Count,
                completedCount = a.Students.Count(s => s.Status == AssignmentStudentStatus.Completed),
            })
            .ToListAsync();

        // Questions created by tutors
        var totalQuestions = await _db.Questions
            .Where(q => q.CreatedByTutorId != null && filterIds.Contains(q.CreatedByTutorId.Value) && !q.IsDeleted)
            .CountAsync();

        var questions = await _db.Questions
            .Include(q => q.CreatedByTutor)
            .Include(q => q.Topic)
            .Where(q => q.CreatedByTutorId != null && filterIds.Contains(q.CreatedByTutorId.Value) && !q.IsDeleted)
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(q => new
            {
                q.Id, q.Text, difficulty = q.Difficulty.ToString(), q.IsPrivate, q.CreatedAt,
                tutorName = q.CreatedByTutor!.Name,
                tutorUserId = q.CreatedByTutorId,
                topicName = q.Topic.Name,
                examTypeCode = q.Topic.Section != null ? q.Topic.Section.ExamTypeCode : "",
            })
            .ToListAsync();

        return Ok(new
        {
            assignments = new { items = assignments, total = totalAssignments },
            questions = new { items = questions, total = totalQuestions },
            page, pageSize
        });
    }

    // ─── School Invite Code ─────────────────────────────

    /// <summary>Generate or regenerate school invite code</summary>
    [HttpPost("invite-code")]
    public async Task<IActionResult> GenerateInviteCode()
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[8];
        rng.GetBytes(bytes);
        var code = new char[8];
        for (int i = 0; i < 8; i++)
            code[i] = chars[bytes[i] % chars.Length];

        school.SchoolInviteCode = new string(code);
        school.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { inviteCode = school.SchoolInviteCode, requireApproval = school.RequireApproval });
    }

    /// <summary>Get current school invite code</summary>
    [HttpGet("invite-code")]
    public async Task<IActionResult> GetInviteCode()
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        return Ok(new { inviteCode = school.SchoolInviteCode, requireApproval = school.RequireApproval });
    }

    /// <summary>Toggle whether school invite code requires admin approval</summary>
    [HttpPut("invite-code/approval")]
    public async Task<IActionResult> ToggleApproval([FromBody] ToggleApprovalDto dto)
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        school.RequireApproval = dto.RequireApproval;
        school.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { requireApproval = school.RequireApproval });
    }

    /// <summary>Get school subscription status</summary>
    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription()
    {
        var school = await GetOwnedSchool();
        if (school == null) return NotFound(new { error = "You don't own a school" });

        return Ok(new
        {
            subscriptionExpiresAt = school.SubscriptionExpiresAt,
            subscriptionPaidAt = school.SubscriptionPaidAt,
            isActive = school.SubscriptionExpiresAt != null && school.SubscriptionExpiresAt > DateTime.UtcNow
        });
    }
}

public record ToggleApprovalDto(bool RequireApproval);
