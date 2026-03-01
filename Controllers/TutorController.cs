using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/tutors")]
[Authorize]
[ApiVersion("1.0")]
public class TutorController : ControllerBase
{
    private readonly ITutorService _tutorService;

    public TutorController(ITutorService tutorService)
    {
        _tutorService = tutorService;
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

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id)) return id;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
