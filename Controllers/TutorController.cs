using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;
using UniStart.Hubs;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/tutors")]
[Authorize]
[ApiVersion("1.0")]
public class TutorController : ControllerBase
{
    private readonly ITutorService _tutorService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly UniStartDbContext _db;

    public TutorController(ITutorService tutorService, IHubContext<ChatHub> hubContext, UniStartDbContext db)
    {
        _tutorService = tutorService;
        _hubContext = hubContext;
        _db = db;
    }

    /// <summary>Каталог тьюторов с фильтрами и пагинацией</summary>
    [HttpGet]
    public async Task<IActionResult> GetTutors(
        [FromQuery] string? search,
        [FromQuery] string? exam,
        [FromQuery] string? sort,
        [FromQuery] bool? available,
        [FromQuery] int? schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _tutorService.GetTutorsAsync(search, exam, sort, available, page, pageSize, schoolId);
        return Ok(result);
    }

    /// <summary>Полный профиль тьютора</summary>
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetTutorProfile(int userId)
    {
        var profile = await _tutorService.GetTutorProfileAsync(userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    /// <summary>Обновить свой профиль тьютора</summary>
    [HttpPut("profile")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateTutorProfileDto dto)
    {
        var userId = GetUserId();
        var profile = await _tutorService.UpdateMyProfileAsync(userId, dto);
        return Ok(profile);
    }

    /// <summary>Request verification from platform admin (for free tutors)</summary>
    [HttpPost("request-verification")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> RequestVerification()
    {
        var userId = GetUserId();
        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound(new { error = "Profile not found" });
        if (profile.IsVerified) return Ok(new { alreadyVerified = true });
        if (profile.VerificationRequestedAt != null)
            return Ok(new { alreadyRequested = true, requestedAt = profile.VerificationRequestedAt });

        profile.VerificationRequestedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { requested = true, requestedAt = profile.VerificationRequestedAt });
    }

    /// <summary>Установить расписание</summary>
    [HttpPut("schedule")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> SetSchedule([FromBody] SetScheduleDto dto)
    {
        var userId = GetUserId();
        var profile = await _tutorService.SetScheduleAsync(userId, dto);
        return Ok(profile);
    }

    /// <summary>Оставить отзыв тьютору</summary>
    [HttpPost("{userId:int}/reviews")]
    public async Task<IActionResult> LeaveReview(int userId, [FromBody] CreateReviewDto dto)
    {
        var studentId = GetUserId();
        var review = await _tutorService.LeaveReviewAsync(studentId, userId, dto);
        return Ok(review);
    }

    /// <summary>Мои студенты (для тьютора)</summary>
    [HttpGet("my-students")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetMyStudents()
    {
        var userId = GetUserId();
        var students = await _tutorService.GetMyStudentsAsync(userId);
        return Ok(students);
    }

    /// <summary>Ожидающие заявки от студентов</summary>
    [HttpGet("requests/pending")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = GetUserId();
        var requests = await _tutorService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }

    /// <summary>Принять заявку студента</summary>
    [HttpPost("requests/{conversationId:int}/accept")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> AcceptStudent(int conversationId)
    {
        var userId = GetUserId();
        var result = await _tutorService.AcceptStudentAsync(userId, conversationId);

        // Push status change + system message to student via SignalR
        var studentId = await _tutorService.GetStudentIdByConversationAsync(conversationId);
        if (studentId > 0)
        {
            await _hubContext.Clients.Group($"user_{studentId}").SendAsync(
                "ConversationStatusChanged", conversationId, "Active");

            if (!string.IsNullOrEmpty(result.SystemMessage))
            {
                var sysMsg = new MessageDto(
                    0, conversationId, userId, "", result.SystemMessage,
                    DateTime.UtcNow, null, false, "System", false);
                await _hubContext.Clients.Group($"user_{studentId}").SendAsync("ReceiveMessage", sysMsg);
            }
        }

        return Ok(result);
    }

    /// <summary>Отклонить заявку студента</summary>
    [HttpPost("requests/{conversationId:int}/decline")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> DeclineStudent(int conversationId, [FromBody] DeclineRequestDto? dto)
    {
        var userId = GetUserId();
        var result = await _tutorService.DeclineStudentAsync(userId, conversationId, dto?.Reason);

        // Push status change + system message to student via SignalR
        var studentId = await _tutorService.GetStudentIdByConversationAsync(conversationId);
        if (studentId > 0)
        {
            await _hubContext.Clients.Group($"user_{studentId}").SendAsync(
                "ConversationStatusChanged", conversationId, "Declined");

            if (!string.IsNullOrEmpty(result.SystemMessage))
            {
                var sysMsg = new MessageDto(
                    0, conversationId, userId, "", result.SystemMessage,
                    DateTime.UtcNow, null, false, "System", false);
                await _hubContext.Clients.Group($"user_{studentId}").SendAsync("ReceiveMessage", sysMsg);
            }
        }

        return Ok(result);
    }

    /// <summary>Список партнёрских школ</summary>
    [HttpGet("schools")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchools()
    {
        var schools = await _tutorService.GetSchoolsAsync();
        return Ok(schools);
    }

    /// <summary>Брендинг школы по слагу (для White Label субдоменов)</summary>
    [HttpGet("schools/branding")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchoolBranding([FromQuery] string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return BadRequest(new { error = "slug required" });
        var branding = await _tutorService.GetSchoolBrandingAsync(slug);
        if (branding == null) return NotFound();
        return Ok(branding);
    }

    /// <summary>Школа с тьюторами</summary>
    [HttpGet("schools/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchool(string slug)
    {
        var school = await _tutorService.GetSchoolAsync(slug);
        if (school == null) return NotFound();
        return Ok(school);
    }

    // ═══ School Management (Этап 4) ═════════════════════════

    /// <summary>Создать школу (тьютор)</summary>
    [HttpPost("schools")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> CreateSchool([FromBody] CreateSchoolDto dto)
    {
        var userId = GetUserId();
        var result = await _tutorService.CreateSchoolAsync(userId, dto);
        return Ok(result);
    }

    /// <summary>Получить свою школу (владелец)</summary>
    [HttpGet("my-school")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetMySchool()
    {
        var userId = GetUserId();
        var result = await _tutorService.GetMySchoolAsync(userId);
        if (result == null) return Ok(new { school = (object?)null });
        return Ok(result);
    }

    /// <summary>Обновить свою школу (владелец)</summary>
    [HttpPut("my-school")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> UpdateMySchool([FromBody] UpdateSchoolDto dto)
    {
        var userId = GetUserId();
        var result = await _tutorService.UpdateSchoolAsync(userId, dto);
        if (result == null) return NotFound(new { error = "School not found or not owner" });
        return Ok(result);
    }

    /// <summary>Добавить тьютора в школу (владелец)</summary>
    [HttpPost("my-school/tutors/{tutorUserId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> AddTutorToSchool(int tutorUserId)
    {
        var userId = GetUserId();
        var result = await _tutorService.AddTutorToSchoolAsync(userId, tutorUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Удалить тьютора из школы (владелец)</summary>
    [HttpDelete("my-school/tutors/{tutorUserId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> RemoveTutorFromSchool(int tutorUserId)
    {
        var userId = GetUserId();
        var result = await _tutorService.RemoveTutorFromSchoolAsync(userId, tutorUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ═══ Tutor-Student Binding ═══════════════════════════════

    /// <summary>Генерация инвайт-кода (тьютор)</summary>
    [HttpPost("invite-code")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GenerateInviteCode()
    {
        var userId = GetUserId();
        var result = await _tutorService.GenerateInviteCodeAsync(userId);
        return Ok(result);
    }

    /// <summary>Получить текущий инвайт-код (тьютор)</summary>
    [HttpGet("invite-code")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetInviteCode()
    {
        var userId = GetUserId();
        var result = await _tutorService.GetInviteCodeAsync(userId);
        if (result == null) return Ok(new { inviteCode = (string?)null });
        return Ok(result);
    }

    /// <summary>Список всех инвайт-кодов тьютора (S-6)</summary>
    [HttpGet("invite-codes")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetInviteCodes()
    {
        var userId = GetUserId();
        return Ok(await _tutorService.GetInviteCodesAsync(userId));
    }

    /// <summary>Создать инвайт-код с параметрами (S-6)</summary>
    [HttpPost("invite-codes")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> CreateInviteCode([FromBody] CreateInviteCodeDto dto)
    {
        var userId = GetUserId();
        return Ok(await _tutorService.CreateInviteCodeAsync(userId, dto));
    }

    /// <summary>Деактивировать инвайт-код (S-6)</summary>
    [HttpDelete("invite-codes/{codeId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> DeactivateInviteCode(int codeId)
    {
        var userId = GetUserId();
        var ok = await _tutorService.DeactivateInviteCodeAsync(userId, codeId);
        return ok ? Ok() : NotFound();
    }

    /// <summary>Привязаться к тьютору по инвайт-коду (ученик)</summary>
    [HttpPost("link")]
    public async Task<IActionResult> LinkByInviteCode([FromBody] LinkByInviteDto dto)
    {
        var userId = GetUserId();
        var result = await _tutorService.LinkStudentByCodeAsync(userId, dto.InviteCode);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Отвязать ученика (тьютор)</summary>
    [HttpDelete("students/{studentUserId}/unlink")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> UnlinkStudent(int studentUserId)
    {
        var userId = GetUserId();
        var result = await _tutorService.UnlinkStudentAsync(userId, studentUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Отвязаться от тьютора (ученик)</summary>
    [HttpDelete("unlink")]
    public async Task<IActionResult> UnlinkFromTutor()
    {
        var userId = GetUserId();
        var result = await _tutorService.UnlinkFromTutorAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Привязанные ученики (тьютор)</summary>
    [HttpGet("linked-students")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetLinkedStudents()
    {
        var userId = GetUserId();
        var students = await _tutorService.GetLinkedStudentsAsync(userId);
        return Ok(students);
    }

    /// <summary>Мой привязанный тьютор (ученик)</summary>
    [HttpGet("my-tutor")]
    public async Task<IActionResult> GetMyLinkedTutor()
    {
        var userId = GetUserId();
        var tutor = await _tutorService.GetLinkedTutorAsync(userId);
        return Ok(new { tutor });
    }

    // ═══════════════════════════════════════════════════════
    //  TUTOR QUESTION MANAGEMENT (Этап 2)
    // ═══════════════════════════════════════════════════════

    /// <summary>Создать вопрос (тьютор)</summary>
    [HttpPost("questions")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        var userId = GetUserId();
        var result = await _tutorService.CreateQuestionAsync(userId, dto);
        return Ok(result);
    }

    /// <summary>Обновить вопрос (тьютор)</summary>
    [HttpPut("questions/{questionId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuestionDto dto)
    {
        var userId = GetUserId();
        var result = await _tutorService.UpdateQuestionAsync(userId, questionId, dto);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>Удалить вопрос (тьютор)</summary>
    [HttpDelete("questions/{questionId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> DeleteQuestion(int questionId)
    {
        var userId = GetUserId();
        var deleted = await _tutorService.DeleteQuestionAsync(userId, questionId);
        return deleted ? Ok() : NotFound();
    }

    /// <summary>Мои вопросы (тьютор)</summary>
    [HttpGet("questions")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetMyQuestions(
        [FromQuery] string? search, [FromQuery] string? examType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _tutorService.GetMyQuestionsAsync(userId, search, examType, page, pageSize);
        return Ok(result);
    }

    /// <summary>Деталь вопроса (тьютор)</summary>
    [HttpGet("questions/{questionId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetQuestion(int questionId)
    {
        var userId = GetUserId();
        var result = await _tutorService.GetQuestionByIdAsync(userId, questionId);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>Список тем для тьютора</summary>
    [HttpGet("topics")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetTopics()
    {
        var result = await _tutorService.GetTopicsAsync();
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════
    //  ASSIGNMENTS (Sprint 7 Этап 3)
    // ═══════════════════════════════════════════════════════

    /// <summary>Создать задание</summary>
    [HttpPost("assignments")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto dto)
    {
        var userId = GetUserId();
        try
        {
            var result = await _tutorService.CreateAssignmentAsync(userId, dto);
            return Created($"/api/tutors/assignments/{result.Id}", result);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Обновить задание</summary>
    [HttpPut("assignments/{assignmentId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> UpdateAssignment(int assignmentId, [FromBody] UpdateAssignmentDto dto)
    {
        var userId = GetUserId();
        var result = await _tutorService.UpdateAssignmentAsync(userId, assignmentId, dto);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>Удалить задание</summary>
    [HttpDelete("assignments/{assignmentId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> DeleteAssignment(int assignmentId)
    {
        var userId = GetUserId();
        var ok = await _tutorService.DeleteAssignmentAsync(userId, assignmentId);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Список заданий тьютора</summary>
    [HttpGet("assignments")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetAssignments()
    {
        var userId = GetUserId();
        var result = await _tutorService.GetAssignmentsAsync(userId);
        return Ok(result);
    }

    /// <summary>Деталь задания (тьютор)</summary>
    [HttpGet("assignments/{assignmentId}")]
    [Authorize(Roles = "Tutor,Admin")]
    public async Task<IActionResult> GetAssignment(int assignmentId)
    {
        var userId = GetUserId();
        var result = await _tutorService.GetAssignmentAsync(userId, assignmentId);
        return result != null ? Ok(result) : NotFound();
    }

    // ── Student-facing assignment endpoints ──

    /// <summary>Мои задания (ученик)</summary>
    [HttpGet("my-assignments")]
    public async Task<IActionResult> GetMyAssignments()
    {
        var userId = GetUserId();
        var result = await _tutorService.GetStudentAssignmentsAsync(userId);
        return Ok(result);
    }

    /// <summary>Деталь задания (ученик)</summary>
    [HttpGet("my-assignments/{assignmentId}")]
    public async Task<IActionResult> GetMyAssignment(int assignmentId)
    {
        var userId = GetUserId();
        var result = await _tutorService.GetStudentAssignmentAsync(userId, assignmentId);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>Ответить на вопрос задания (ученик)</summary>
    [HttpPost("my-assignments/{assignmentId}/answer")]
    public async Task<IActionResult> SubmitAssignmentAnswer(int assignmentId, [FromBody] SubmitAssignmentAnswerDto dto)
    {
        var userId = GetUserId();
        try
        {
            var result = await _tutorService.SubmitAssignmentAnswerAsync(userId, assignmentId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Get allowed specializations and sections for the current tutor</summary>
    [HttpGet("my-school-specs")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GetMySchoolSpecs()
    {
        var userId = GetUserId();
        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        string[] allowedExams;

        if (profile?.SchoolId != null)
        {
            var school = await _db.TutorSchools.FindAsync(profile.SchoolId);
            allowedExams = school?.Specializations?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? Array.Empty<string>();
        }
        else
        {
            allowedExams = new[] { "SAT", "TOEFL", "IELTS", "NUET", "CSCA" };
        }

        var sections = await _db.ExamSections
            .Where(s => allowedExams.Contains(s.ExamTypeCode))
            .Select(s => new { s.Id, s.ExamTypeCode, s.Name })
            .ToListAsync();

        return Ok(new { allowedExams, sections });
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id)) return id;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
