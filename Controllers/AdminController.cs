using System.Diagnostics;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement.Mvc;
using System.Security.Claims;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ApiVersion("1.0")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _svc;
    private readonly IAuditService _audit;
    private readonly UniStartDbContext _db;
    private readonly HealthCheckService _healthCheck;
    private readonly IEmailService _email;
    private readonly IImageUploadService _imageUpload;
    private readonly IMockExamService _mockExams;

    public AdminController(IAdminService svc, IAuditService audit, UniStartDbContext db, HealthCheckService healthCheck, IEmailService email, IImageUploadService imageUpload, IMockExamService mockExams)
    {
        _svc = svc;
        _audit = audit;
        _db = db;
        _healthCheck = healthCheck;
        _email = email;
        _imageUpload = imageUpload;
        _mockExams = mockExams;
    }

    /// <summary>Upload an image to Cloudflare R2 and return its public URL</summary>
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken ct)
    {
        try
        {
            var url = await _imageUpload.UploadAsync(file, ct);
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

    /// <summary>List questions with optional filters</summary>
    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] string? examTypeCode = null,
        [FromQuery] string? topic = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? section = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null)
    {
        var result = await _svc.GetQuestionsAsync(examTypeCode, topic, difficulty, section, page, pageSize, search);
        return Ok(result);
    }

    /// <summary>Get question details by ID</summary>
    [HttpGet("questions/{id:int}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        var result = await _svc.GetQuestionByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Create a new question</summary>
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        try
        {
            var result = await _svc.CreateQuestionAsync(dto);
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Create", "Question", result.Id.ToString(),
                newValues: new { result.Id, result.TopicName, result.Difficulty, result.Text },
                ipAddress: GetClientIp());
            return CreatedAtAction(nameof(GetQuestion), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an existing question</summary>
    [HttpPut("questions/{id:int}")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto dto)
    {
        var before = await _svc.GetQuestionByIdAsync(id);
        var result = await _svc.UpdateQuestionAsync(id, dto);
        if (result == null) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Update", "Question", id.ToString(),
            oldValues: before != null ? new { before.Difficulty, before.Text, before.DifficultyParam } : null,
            newValues: new { result.Difficulty, result.Text, result.DifficultyParam },
            ipAddress: GetClientIp());
        return Ok(result);
    }

    /// <summary>Delete a question</summary>
    [HttpDelete("questions/{id:int}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var before = await _svc.GetQuestionByIdAsync(id);
        var ok = await _svc.DeleteQuestionAsync(id);
        if (!ok) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Delete", "Question", id.ToString(),
            oldValues: before != null ? new { before.TopicName, before.Difficulty, before.Text } : null,
            ipAddress: GetClientIp());
        return NoContent();
    }

    /// <summary>Bulk import questions (JSON array)</summary>
    [HttpPost("questions/import")]
    public async Task<IActionResult> BulkImport([FromBody] BulkImportDto dto)
    {
        var result = await _svc.BulkImportAsync(dto);
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "BulkImport", "Question", null,
            newValues: new { result.Total, result.Imported, result.Failed },
            ipAddress: GetClientIp());
        return Ok(result);
    }

    /// <summary>Question database stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _svc.GetStatsAsync();
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════
    //  USERS
    // ═══════════════════════════════════════════════════════

    /// <summary>List all users with optional filters</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool includeDeleted = false)
    {
        var result = await _svc.GetUsersAsync(role, search, page, pageSize, includeDeleted);
        return Ok(result);
    }

    /// <summary>Get user details by ID</summary>
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _svc.GetUserByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Update user (role, subscription, etc.)</summary>
    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        try
        {
            var before = await _svc.GetUserByIdAsync(id);
            var result = await _svc.UpdateUserAsync(id, dto);
            if (result == null) return NotFound();
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Update", "User", id.ToString(),
                oldValues: before != null ? new { before.Role, before.SubscriptionTier, before.Email } : null,
                newValues: new { result.Role, result.SubscriptionTier, result.Email },
                ipAddress: GetClientIp());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Delete a user and all related data</summary>
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var before = await _svc.GetUserByIdAsync(id);
            var ok = await _svc.DeleteUserAsync(id);
            if (!ok) return NotFound();
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Delete", "User", id.ToString(),
                oldValues: before != null ? new { before.Email, before.Name, before.Role } : null,
                ipAddress: GetClientIp());
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>User statistics summary</summary>
    [HttpGet("users/stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var result = await _svc.GetUserStatsAsync();
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════
    //  DASHBOARD & TOPICS
    // ═══════════════════════════════════════════════════════

    /// <summary>Admin dashboard with overview stats</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _svc.GetDashboardAsync();
        return Ok(result);
    }

    /// <summary>List all topics with question counts</summary>
    [HttpGet("topics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTopics()
    {
        var result = await _svc.GetTopicsAsync();
        return Ok(result);
    }

    /// <summary>Create a new topic</summary>
    [HttpPost("topics")]
    public async Task<IActionResult> CreateTopic([FromBody] CreateTopicDto dto)
    {
        try
        {
            var result = await _svc.CreateTopicAsync(dto);
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Create", "Topic", result.Id.ToString(),
                newValues: new { result.Name, result.SectionName, result.ExamTypeCode },
                ipAddress: GetClientIp());
            return Created($"/api/admin/topics", result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an existing topic name</summary>
    [HttpPut("topics/{id:int}")]
    public async Task<IActionResult> UpdateTopic(int id, [FromBody] UpdateTopicDto dto)
    {
        try
        {
            var result = await _svc.UpdateTopicAsync(id, dto);
            if (result == null) return NotFound();
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Update", "Topic", id.ToString(),
                newValues: new { result.Name },
                ipAddress: GetClientIp());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Delete a topic and soft-delete all its questions</summary>
    [HttpDelete("topics/{id:int}")]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        var ok = await _svc.DeleteTopicAsync(id);
        if (!ok) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Delete", "Topic", id.ToString(),
            ipAddress: GetClientIp());
        return NoContent();
    }

    /// <summary>Delete all questions of a topic while keeping the topic itself</summary>
    [HttpDelete("topics/{id:int}/questions")]
    public async Task<IActionResult> ClearTopicQuestions(int id)
    {
        var removed = await _svc.ClearTopicQuestionsAsync(id);
        if (removed == null) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "ClearQuestions", "Topic", id.ToString(),
            ipAddress: GetClientIp());
        return Ok(new { removed });
    }

    /// <summary>List all exam sections (for dropdowns)</summary>
    [HttpGet("sections")]
    public async Task<IActionResult> GetSections()
    {
        var result = await _svc.GetSectionsAsync();
        return Ok(result);
    }

    /// <summary>Create a new exam section</summary>
    [HttpPost("sections")]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto dto)
    {
        try
        {
            var result = await _svc.CreateSectionAsync(dto);
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Create", "Section", result.Id.ToString(),
                newValues: new { result.Name, result.ExamTypeCode },
                ipAddress: GetClientIp());
            return Created($"/api/admin/sections", result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an existing section name</summary>
    [HttpPut("sections/{id:int}")]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateSectionDto dto)
    {
        try
        {
            var result = await _svc.UpdateSectionAsync(id, dto);
            if (result == null) return NotFound();
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Update", "Section", id.ToString(),
                newValues: new { result.Name },
                ipAddress: GetClientIp());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Delete a section and cascade soft-delete all its topics' questions</summary>
    [HttpDelete("sections/{id:int}")]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var ok = await _svc.DeleteSectionAsync(id);
        if (!ok) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Delete", "Section", id.ToString(),
            ipAddress: GetClientIp());
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════
    //  EXAM TYPES — CRUD
    // ═══════════════════════════════════════════════════════

    /// <summary>List all exam types</summary>
    [HttpGet("exam-types")]
    public async Task<IActionResult> GetExamTypes()
    {
        var result = await _svc.GetExamTypesAsync();
        return Ok(result);
    }

    /// <summary>Create a new exam type</summary>
    [HttpPost("exam-types")]
    public async Task<IActionResult> CreateExamType([FromBody] CreateExamTypeDto dto)
    {
        var result = await _svc.CreateExamTypeAsync(dto);
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Create", "ExamType", result.Code,
            newValues: new { result.Code, result.Name }, ipAddress: GetClientIp());
        return CreatedAtAction(nameof(GetExamTypes), result);
    }

    /// <summary>Update an exam type's name</summary>
    [HttpPut("exam-types/{code}")]
    public async Task<IActionResult> UpdateExamType(string code, [FromBody] UpdateExamTypeDto dto)
    {
        var result = await _svc.UpdateExamTypeAsync(code, dto);
        if (result == null) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Update", "ExamType", code,
            newValues: new { result.Name }, ipAddress: GetClientIp());
        return Ok(result);
    }

    /// <summary>Delete an exam type (cascades to sections, topics, questions)</summary>
    [HttpDelete("exam-types/{code}")]
    public async Task<IActionResult> DeleteExamType(string code)
    {
        var ok = await _svc.DeleteExamTypeAsync(code);
        if (!ok) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Delete", "ExamType", code,
            ipAddress: GetClientIp());
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════
    //  BLOCK / SUSPEND (OP-14)
    // ═══════════════════════════════════════════════════════

    /// <summary>Block a user</summary>
    [HttpPost("users/{id:int}/block")]
    public async Task<IActionResult> BlockUser(int id, [FromBody] BlockUserDto? dto = null)
    {
        try
        {
            var result = await _svc.BlockUserAsync(id, dto?.Reason);
            if (result == null) return NotFound();
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "Block", "User", id.ToString(),
                newValues: new { Reason = dto?.Reason },
                ipAddress: GetClientIp());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Unblock a user</summary>
    [HttpPost("users/{id:int}/unblock")]
    public async Task<IActionResult> UnblockUser(int id)
    {
        var result = await _svc.UnblockUserAsync(id);
        if (result == null) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Unblock", "User", id.ToString(), ipAddress: GetClientIp());
        return Ok(result);
    }

    /// <summary>Grant the user another one-time free mock run.</summary>
    [HttpPost("users/{id:int}/reset-free-mock")]
    public async Task<IActionResult> ResetFreeMock(int id)
    {
        var result = await _svc.ResetFreeMockAsync(id);
        if (result == null) return NotFound();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "ResetFreeMock", "User", id.ToString(), ipAddress: GetClientIp());
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════
    //  AUDIT LOGS (OP-7)
    // ═══════════════════════════════════════════════════════

    /// <summary>Query audit logs with filters</summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _audit.GetLogsAsync(action, entityType, userId, from, to, page, pageSize);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════
    //  RESTORE (Soft Delete — OP-9)
    // ═══════════════════════════════════════════════════════

    /// <summary>Restore a soft-deleted question</summary>
    [HttpPost("questions/{id:int}/restore")]
    public async Task<IActionResult> RestoreQuestion(int id)
    {
        var ok = await _svc.RestoreQuestionAsync(id);
        if (!ok) return NotFound(new { error = "Question not found or not deleted" });
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Restore", "Question", id.ToString(), ipAddress: GetClientIp());
        return Ok(new { message = "Question restored" });
    }

    /// <summary>Restore a soft-deleted user</summary>
    [HttpPost("users/{id:int}/restore")]
    public async Task<IActionResult> RestoreUser(int id)
    {
        var ok = await _svc.RestoreUserAsync(id);
        if (!ok) return NotFound(new { error = "User not found or not deleted" });
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "Restore", "User", id.ToString(), ipAddress: GetClientIp());
        return Ok(new { message = "User restored" });
    }

    // ═══════════════════════════════════════════════════════
    //  TRASH / RECYCLE BIN
    // ═══════════════════════════════════════════════════════

    /// <summary>List all soft-deleted items (users + questions)</summary>
    [HttpGet("trash")]
    public async Task<IActionResult> GetTrash()
    {
        var result = await _svc.GetTrashAsync();
        return Ok(result);
    }

    /// <summary>Permanently delete a soft-deleted question</summary>
    [HttpDelete("trash/questions/{id:int}")]
    public async Task<IActionResult> HardDeleteQuestion(int id)
    {
        var ok = await _svc.HardDeleteQuestionAsync(id);
        if (!ok) return NotFound(new { error = "Question not found or not in trash" });
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "HardDelete", "Question", id.ToString(), ipAddress: GetClientIp());
        return NoContent();
    }

    /// <summary>Permanently delete a soft-deleted user</summary>
    [HttpDelete("trash/users/{id:int}")]
    public async Task<IActionResult> HardDeleteUser(int id)
    {
        try
        {
            var ok = await _svc.HardDeleteUserAsync(id);
            if (!ok) return NotFound(new { error = "User not found or not in trash" });
            var (adminId, email) = GetCurrentAdmin();
            await _audit.LogAsync(adminId, email, "HardDelete", "User", id.ToString(), ipAddress: GetClientIp());
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Permanently delete all items in trash</summary>
    [HttpDelete("trash")]
    public async Task<IActionResult> EmptyTrash()
    {
        var count = await _svc.EmptyTrashAsync();
        var (adminId, email) = GetCurrentAdmin();
        await _audit.LogAsync(adminId, email, "EmptyTrash", "All", null,
            newValues: new { PurgedCount = count },
            ipAddress: GetClientIp());
        return Ok(new { message = $"Trash emptied: {count} records permanently removed", count });
    }

    // ═══════════════════════════════════════════════════════
    //  CSV EXPORT (OP-18)
    // ═══════════════════════════════════════════════════════

    /// <summary>Export all questions as CSV</summary>
    [HttpGet("questions/export")]
    [FeatureGate("CsvExport")]
    public async Task<IActionResult> ExportQuestions(
        [FromQuery] string? examTypeCode = null,
        [FromQuery] string? difficulty = null)
    {
        var result = await _svc.GetQuestionsAsync(examTypeCode, null, difficulty, null, 1, 10000);
        var csv = BuildCsv(result.Items, new[]
        {
            ("ID", (Func<QuestionListDto, string>)(q => q.Id.ToString())),
            ("Exam", q => q.ExamTypeCode),
            ("Section", q => q.SectionName),
            ("Topic", q => q.TopicName),
            ("Difficulty", q => q.Difficulty),
            ("b", q => q.DifficultyParam.ToString("F2")),
            ("a", q => q.DiscriminationParam.ToString("F2")),
            ("Answers", q => q.AnswerCount.ToString()),
            ("Text", q => q.Text),
            ("Created", q => q.CreatedAt.ToString("yyyy-MM-dd")),
        });
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "questions.csv");
    }

    /// <summary>Export all users as CSV</summary>
    [HttpGet("users/export")]
    [FeatureGate("CsvExport")]
    public async Task<IActionResult> ExportUsers(
        [FromQuery] string? role = null)
    {
        var result = await _svc.GetUsersAsync(role, null, 1, 10000);
        var csv = BuildCsv(result.Items, new[]
        {
            ("ID", (Func<AdminUserDto, string>)(u => u.Id.ToString())),
            ("Name", u => u.Name),
            ("Email", u => u.Email),
            ("Phone", u => u.PhoneNumber ?? ""),
            ("Role", u => u.Role),
            ("Tier", u => u.SubscriptionTier),
            ("Blocked", u => u.IsBlocked ? "Yes" : "No"),
            ("Onboarding", u => u.HasCompletedOnboarding ? "Yes" : "No"),
            ("TotalAnswers", u => u.TotalAnswers.ToString()),
            ("CorrectAnswers", u => u.CorrectAnswers.ToString()),
            ("Sessions", u => u.TestSessions.ToString()),
            ("Created", u => u.CreatedAt.ToString("yyyy-MM-dd")),
        });
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "users.csv");
    }

    // ═══════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════

    private static string BuildCsv<T>(List<T> items, (string header, Func<T, string> getValue)[] columns)
    {
        var sb = new System.Text.StringBuilder();
        // BOM for Excel UTF-8 detection
        sb.Append('\uFEFF');
        // Header
        sb.AppendLine(string.Join(",", columns.Select(c => CsvEscape(c.header))));
        // Rows
        foreach (var item in items)
            sb.AppendLine(string.Join(",", columns.Select(c => CsvEscape(c.getValue(item)))));
        return sb.ToString();
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    // ───────────────────────────────────────────────────────
    //  BACKGROUND JOBS STATUS (OP-12)
    // ───────────────────────────────────────────────────────

    /// <summary>Get status of all recurring background jobs</summary>
    [HttpGet("jobs/status")]
    public IActionResult GetJobsStatus()
    {
        using var connection = JobStorage.Current.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();

        var result = recurringJobs.Select(j => new
        {
            j.Id,
            cron = j.Cron,
            queue = j.Queue,
            lastExecution = j.LastExecution,
            nextExecution = j.NextExecution,
            lastJobId = j.LastJobId,
            lastJobState = j.LastJobId != null
                ? connection.GetJobData(j.LastJobId)?.State
                : null,
            createdAt = j.CreatedAt
        });

        return Ok(new { jobs = result });
    }

    /// <summary>Trigger a recurring job manually</summary>
    [HttpPost("jobs/{jobId}/trigger")]
    public IActionResult TriggerJob(string jobId)
    {
        RecurringJob.TriggerJob(jobId);
        return Ok(new { message = $"Job '{jobId}' triggered." });
    }

    // ───────────────────────────────────────────────────────
    //  SYSTEM HEALTH (OP-23)
    // ───────────────────────────────────────────────────────

    /// <summary>Get comprehensive system health overview</summary>
    [HttpGet("system/health")]
    public async Task<IActionResult> GetSystemHealth()
    {
        // 1. Health checks
        var healthReport = await _healthCheck.CheckHealthAsync();

        // 2. Database stats
        var totalUsers = await _db.Users.CountAsync();
        var totalQuestions = await _db.Questions.CountAsync();
        var totalAnswers = await _db.UserAnswers.CountAsync();
        var totalSessions = await _db.TestSessions.CountAsync();
        var activeUsersToday = await _db.UserAnswers
            .Where(a => a.AnsweredAt.Date == DateTime.UtcNow.Date)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();
        var answersToday = await _db.UserAnswers
            .Where(a => a.AnsweredAt.Date == DateTime.UtcNow.Date)
            .CountAsync();

        // 3. Recurring jobs
        object? jobsInfo = null;
        try
        {
            using var conn = JobStorage.Current.GetConnection();
            var recurringJobs = conn.GetRecurringJobs();
            jobsInfo = recurringJobs.Select(j => new
            {
                j.Id,
                cron = j.Cron,
                lastExecution = j.LastExecution,
                nextExecution = j.NextExecution,
                lastJobState = j.LastJobId != null
                    ? conn.GetJobData(j.LastJobId)?.State
                    : null
            });
        }
        catch { /* Hangfire may not be ready */ }

        // 4. Process info
        var process = Process.GetCurrentProcess();

        return Ok(new
        {
            status = healthReport.Status.ToString(),
            healthChecks = healthReport.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                error = e.Value.Exception?.Message
            }),
            database = new
            {
                totalUsers,
                totalQuestions,
                totalAnswers,
                totalSessions,
                activeUsersToday,
                answersToday
            },
            recurringJobs = jobsInfo,
            system = new
            {
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                dotnetVersion = Environment.Version.ToString(),
                machineName = Environment.MachineName,
                uptime = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMinutes,
                memoryMB = process.WorkingSet64 / 1024.0 / 1024.0,
                threadCount = process.Threads.Count,
                serverTime = DateTime.UtcNow
            }
        });
    }

    // ───────────────────────────────────────────────────────
    //  USER ACTIVITY (OP-23)
    // ───────────────────────────────────────────────────────

    /// <summary>Get detailed activity for a specific user</summary>
    [HttpGet("users/{id:int}/activity")]
    public async Task<IActionResult> GetUserActivity(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.NotificationPreferences)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound(new { error = "User not found" });

        // Recent sessions
        var sessionsQuery = _db.TestSessions
            .Where(s => s.UserId == id)
            .OrderByDescending(s => s.StartedAt);
        var totalSessions = await sessionsQuery.CountAsync();
        var sessions = await sessionsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.StartedAt,
                s.CompletedAt,
                s.TotalQuestions,
                s.CorrectCount,
                isCompleted = s.CompletedAt != null,
                s.ExamTypeCode
            })
            .ToListAsync();

        // Summary stats
        var totalAnswers = await _db.UserAnswers.CountAsync(a => a.UserId == id);
        var correctAnswers = await _db.UserAnswers
            .CountAsync(a => a.UserId == id && a.AnswerOption.IsCorrect);

        // Mock-exam activity (separate tables) — folded into the stats so mock-only
        // users show real numbers, not zeros.
        var mockAnswered = await _db.MockExamAnswers
            .CountAsync(a => a.Attempt.UserId == id && (a.SelectedOptionId != null || a.SelectedOptions.Any()));
        var mockCorrect = await _db.MockExamAnswers
            .CountAsync(a => a.Attempt.UserId == id && a.IsCorrect);
        var mockAttemptCount = await _db.MockExamAttempts.CountAsync(a => a.UserId == id);
        totalAnswers += mockAnswered;
        correctAnswers += mockCorrect;

        // "Last activity" = last time the user was present on the platform.
        var lastActivity = user.LastSeenAt;

        // The user's mock attempts (sessions) with score + status, newest first.
        var mockSessions = await _db.MockExamAttempts
            .Where(a => a.UserId == id)
            .OrderByDescending(a => a.StartedAt)
            .Take(50)
            .Select(a => new
            {
                a.Id,
                title = a.MockExam.Title,
                a.MockExam.ExamTypeCode,
                a.StartedAt,
                a.CompletedAt,
                a.TotalScore,
                a.Status,
            })
            .ToListAsync();

        return Ok(new
        {
            user = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.SubscriptionTier,
                user.CreatedAt,
                user.IsBlocked,
                user.BlockReason
            },
            summary = new
            {
                totalAnswers,
                correctAnswers,
                accuracy = totalAnswers > 0 ? Math.Round((double)correctAnswers / totalAnswers * 100, 1) : 0,
                totalSessions = totalSessions + mockAttemptCount,
                lastActivity
            },
            sessions = new
            {
                items = sessions,
                totalCount = totalSessions,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalSessions / pageSize)
            },
            mockSessions
        });
    }

    /// <summary>Admin: full answer review for any completed mock attempt.</summary>
    [HttpGet("mock-attempts/{attemptId:int}/review")]
    public async Task<IActionResult> GetMockAttemptReview(int attemptId)
    {
        var result = await _mockExams.GetResultsForAdminAsync(attemptId);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Admin: a user's purchase history.</summary>
    [HttpGet("users/{id:int}/purchases")]
    public async Task<IActionResult> GetUserPurchases(int id)
    {
        var items = await _db.Purchases
            .Where(p => p.UserId == id)
            .OrderByDescending(p => p.PurchasedAt)
            .Select(p => new
            {
                p.Id, p.ItemType, p.Title, p.Subjects,
                p.Amount, p.Currency, p.Status, p.PurchasedAt
            })
            .ToListAsync();
        return Ok(items);
    }

    private (int userId, string email) GetCurrentAdmin()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var id))
            throw new UnauthorizedAccessException("Admin identity not found");
        return (id, emailClaim);
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

}
