using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
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

    public TutorController(ITutorService tutorService, IHubContext<ChatHub> hubContext)
    {
        _tutorService = tutorService;
        _hubContext = hubContext;
    }

    /// <summary>Каталог тьюторов с фильтрами и пагинацией</summary>
    [HttpGet]
    public async Task<IActionResult> GetTutors(
        [FromQuery] string? search,
        [FromQuery] string? exam,
        [FromQuery] string? sort,
        [FromQuery] bool? available,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _tutorService.GetTutorsAsync(search, exam, sort, available, page, pageSize);
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

    /// <summary>Школа с тьюторами</summary>
    [HttpGet("schools/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchool(string slug)
    {
        var school = await _tutorService.GetSchoolAsync(slug);
        if (school == null) return NotFound();
        return Ok(school);
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

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id)) return id;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
