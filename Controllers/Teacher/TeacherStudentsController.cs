using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniStart.Models.Core;
using UniStart.Services;

namespace UniStart.Controllers.Teacher;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
public class TeacherStudentsController : ControllerBase
{
    private readonly ITeacherStatisticsService _teacherStatsService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeacherStudentsController> _logger;

    public TeacherStudentsController(
        ITeacherStatisticsService teacherStatsService,
        UserManager<ApplicationUser> userManager,
        ILogger<TeacherStudentsController> logger)
    {
        _teacherStatsService = teacherStatsService;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Получить список студентов, которые проходили квизы преподавателя
    /// </summary>
    [HttpGet("students")]
    public async Task<ActionResult<TeacherStudentsResult>> GetStudents(
        [FromQuery] int? quizId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "averageScore",
        [FromQuery] bool desc = true,
        [FromQuery] double? minScore = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var filter = new TeacherStudentsFilter
        {
            TeacherId = userId,
            QuizId = quizId,
            Search = search,
            SortBy = sortBy,
            Desc = desc,
            MinScore = minScore,
            Page = page,
            PageSize = pageSize
        };

        var result = await _teacherStatsService.GetStudentsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Получить детальную статистику по конкретному студенту
    /// </summary>
    [HttpGet("students/{studentId}/stats")]
    public async Task<ActionResult<StudentDetailedStats>> GetStudentStats(string studentId)
    {
        var userId = GetUserId();

        var student = await _userManager.FindByIdAsync(studentId);
        if (student == null)
            return NotFound(new { Message = "Студент не найден" });

        var stats = await _teacherStatsService.GetStudentStatsAsync(userId, studentId);
        
        if (stats == null)
            return Ok(new { Message = "У вас ещё нет данных для этого студента" });

        return Ok(new
        {
            StudentId = studentId,
            Email = student.Email,
            UserName = student.UserName,
            FirstName = student.FirstName ?? "",
            LastName = student.LastName ?? "",
            Stats = stats
        });
    }
}
