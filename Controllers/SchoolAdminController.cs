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

    // ─── Mock Exam Authoring ────────────────────────────

    /// <summary>Exam types available for building mocks</summary>
    [HttpGet("mocks/exam-types")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> GetMockExamTypes()
    {
        var types = await _db.ExamTypes
            .OrderBy(e => e.Name)
            .Select(e => new { code = e.Code, name = e.Name })
            .ToListAsync();
        return Ok(types);
    }

    /// <summary>Exam sections (question pools) for a given exam type</summary>
    [HttpGet("mocks/exam-types/{code}/sections")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> GetMockExamSections(string code)
    {
        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == code)
            .OrderBy(s => s.Name)
            .Select(s => new { id = s.Id, name = s.Name })
            .ToListAsync();
        return Ok(sections);
    }

    /// <summary>List all mock exams with summary info</summary>
    [HttpGet("mocks")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> GetMocks()
    {
        var mocks = await _db.MockExams
            .OrderByDescending(m => m.Id)
            .Select(m => new AdminMockExamListItemDto(
                m.Id, m.ExamTypeCode, m.Title, m.Description, m.TotalTimeMinutes, m.IsActive,
                m.Sections.Count,
                m.Sections.Sum(s => s.QuestionCount),
                m.Attempts.Count))
            .ToListAsync();
        return Ok(mocks);
    }

    /// <summary>Get a single mock exam with its sections</summary>
    [HttpGet("mocks/{id:int}")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> GetMock(int id)
    {
        var mock = await _db.MockExams
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mock == null) return NotFound(new { error = "Mock exam not found" });

        return Ok(MapMockDetail(mock));
    }

    /// <summary>Create a new mock exam</summary>
    [HttpPost("mocks")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> CreateMock([FromBody] SaveMockExamDto dto)
    {
        var validationError = await ValidateMockAsync(dto);
        if (validationError != null) return BadRequest(new { error = validationError });

        var mock = new MockExam
        {
            ExamTypeCode = dto.ExamTypeCode.Trim(),
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            TotalTimeMinutes = dto.TotalTimeMinutes,
            IsActive = dto.IsActive,
            Sections = dto.Sections.Select((s, i) => new MockExamSection
            {
                ExamSectionId = s.ExamSectionId,
                Name = s.Name.Trim(),
                TimeLimitMinutes = s.TimeLimitMinutes,
                QuestionCount = s.QuestionCount,
                SortOrder = s.SortOrder != 0 ? s.SortOrder : i,
                Instructions = string.IsNullOrWhiteSpace(s.Instructions) ? null : s.Instructions.Trim(),
            }).ToList()
        };

        _db.MockExams.Add(mock);
        await _db.SaveChangesAsync();

        var created = await _db.MockExams.Include(m => m.Sections).FirstAsync(m => m.Id == mock.Id);
        return CreatedAtAction(nameof(GetMock), new { id = mock.Id }, MapMockDetail(created));
    }

    /// <summary>Update an existing mock exam (replaces its sections)</summary>
    [HttpPut("mocks/{id:int}")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> UpdateMock(int id, [FromBody] SaveMockExamDto dto)
    {
        var mock = await _db.MockExams
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mock == null) return NotFound(new { error = "Mock exam not found" });

        var validationError = await ValidateMockAsync(dto);
        if (validationError != null) return BadRequest(new { error = validationError });

        mock.ExamTypeCode = dto.ExamTypeCode.Trim();
        mock.Title = dto.Title.Trim();
        mock.Description = dto.Description?.Trim() ?? string.Empty;
        mock.TotalTimeMinutes = dto.TotalTimeMinutes;
        mock.IsActive = dto.IsActive;

        // Replace sections wholesale
        _db.RemoveRange(mock.Sections);
        mock.Sections = dto.Sections.Select((s, i) => new MockExamSection
        {
            MockExamId = mock.Id,
            ExamSectionId = s.ExamSectionId,
            Name = s.Name.Trim(),
            TimeLimitMinutes = s.TimeLimitMinutes,
            QuestionCount = s.QuestionCount,
            SortOrder = s.SortOrder != 0 ? s.SortOrder : i,
            Instructions = string.IsNullOrWhiteSpace(s.Instructions) ? null : s.Instructions.Trim(),
        }).ToList();

        await _db.SaveChangesAsync();

        var updated = await _db.MockExams.Include(m => m.Sections).FirstAsync(m => m.Id == id);
        return Ok(MapMockDetail(updated));
    }

    /// <summary>Toggle a mock exam's active state</summary>
    [HttpPut("mocks/{id:int}/active")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> ToggleMockActive(int id, [FromBody] ToggleMockActiveDto dto)
    {
        var mock = await _db.MockExams.FindAsync(id);
        if (mock == null) return NotFound(new { error = "Mock exam not found" });

        mock.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { id = mock.Id, isActive = mock.IsActive });
    }

    /// <summary>Delete a mock exam (blocked if it has attempts)</summary>
    [HttpDelete("mocks/{id:int}")]
    [Authorize(Roles = "Admin,SchoolAdmin")]
    public async Task<IActionResult> DeleteMock(int id)
    {
        var mock = await _db.MockExams
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mock == null) return NotFound(new { error = "Mock exam not found" });

        var hasAttempts = await _db.MockExamAttempts.AnyAsync(a => a.MockExamId == id);
        if (hasAttempts)
            return Conflict(new { error = "This mock exam has student attempts. Deactivate it instead of deleting." });

        _db.RemoveRange(mock.Sections);
        _db.MockExams.Remove(mock);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateMockAsync(SaveMockExamDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return "Title is required.";
        if (string.IsNullOrWhiteSpace(dto.ExamTypeCode))
            return "Exam type is required.";
        if (dto.TotalTimeMinutes <= 0)
            return "Total time must be greater than zero.";
        if (dto.Sections == null || dto.Sections.Count == 0)
            return "At least one section is required.";

        var examTypeExists = await _db.ExamTypes.AnyAsync(e => e.Code == dto.ExamTypeCode.Trim());
        if (!examTypeExists)
            return "Selected exam type does not exist.";

        foreach (var s in dto.Sections)
        {
            if (string.IsNullOrWhiteSpace(s.Name))
                return "Each section needs a name.";
            if (s.QuestionCount <= 0)
                return $"Section \"{s.Name}\" must have at least one question.";
            if (s.TimeLimitMinutes < 0)
                return $"Section \"{s.Name}\" has an invalid time limit.";
            if (s.ExamSectionId.HasValue)
            {
                var sectionValid = await _db.ExamSections.AnyAsync(
                    es => es.Id == s.ExamSectionId.Value && es.ExamTypeCode == dto.ExamTypeCode.Trim());
                if (!sectionValid)
                    return $"Section \"{s.Name}\" references a pool that doesn't belong to the selected exam type.";
            }
        }
        return null;
    }

    private static AdminMockExamDetailDto MapMockDetail(MockExam mock) => new(
        mock.Id, mock.ExamTypeCode, mock.Title, mock.Description, mock.TotalTimeMinutes, mock.IsActive,
        mock.Sections.OrderBy(s => s.SortOrder).Select(s => new AdminMockSectionDto(
            s.Id, s.ExamSectionId, s.Name, s.TimeLimitMinutes, s.QuestionCount, s.SortOrder, s.Instructions)));
}

public record ToggleApprovalDto(bool RequireApproval);
public record ToggleMockActiveDto(bool IsActive);
