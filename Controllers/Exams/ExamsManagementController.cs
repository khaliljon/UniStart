using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Exams;
using UniStart.Services;

namespace UniStart.Controllers.Exams;

/// <summary>
/// Контроллер для управления экзаменами (CREATE, UPDATE, DELETE, Publish)
/// </summary>
[ApiController]
[Route("api/exams")]
[Authorize(Roles = "Teacher,Admin")]
public class ExamsManagementController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ExamsManagementController> _logger;
    private readonly IExamService _examService;

    public ExamsManagementController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ExamsManagementController> logger,
        IExamService examService)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _examService = examService;
    }

    private async Task<string> GetUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? throw new UnauthorizedAccessException("Пользователь не авторизован");
    }

    /// <summary>
    /// Создать новый экзамен
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ExamDto>> CreateExam([FromBody] CreateExamDto dto)
    {
        try
        {
            var userId = await GetUserId();
            var exam = await _examService.CreateExamAsync(userId, dto);

            return CreatedAtAction("GetExamById", "ExamsQuery", new { id = exam.Id }, new ExamDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                Difficulty = exam.Difficulty,
                Subjects = exam.Subjects?.Select(s => new SubjectDto { Id = s.Id, Name = s.Name }).ToList() ?? new List<SubjectDto>(),
                SubjectIds = exam.Subjects?.Select(s => s.Id).ToList() ?? new List<int>(),
                CountryId = exam.CountryId,
                UniversityId = exam.UniversityId,
                ExamTypeId = exam.ExamTypeId,
                MaxAttempts = exam.MaxAttempts,
                PassingScore = exam.PassingScore,
                IsProctored = exam.IsProctored,
                ShuffleQuestions = exam.ShuffleQuestions,
                ShuffleAnswers = exam.ShuffleAnswers,
                ShowResultsAfter = exam.ShowResultsAfter,
                ShowCorrectAnswers = exam.ShowCorrectAnswers,
                ShowDetailedFeedback = exam.ShowDetailedFeedback,
                TimeLimit = exam.TimeLimit,
                StrictTiming = exam.StrictTiming,
                IsPublished = exam.IsPublished,
                IsPublic = exam.IsPublic,
                MaxScore = exam.MaxScore,
                QuestionCount = exam.Questions?.Count ?? 0,
                CreatedAt = exam.CreatedAt,
                UserId = exam.UserId,
                UserName = exam.User?.UserName ?? "Unknown",
                Questions = exam.Questions?.OrderBy(q => q.OrderIndex).Select(q => new ExamQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Explanation = q.Explanation,
                    QuestionType = q.QuestionType,
                    Points = q.Points,
                    OrderIndex = q.OrderIndex,
                    Answers = q.Answers.OrderBy(a => a.OrderIndex).Select(a => new ExamAnswerDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        OrderIndex = a.OrderIndex
                    }).ToList()
                }).ToList() ?? new List<ExamQuestionDto>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exam");
            return StatusCode(500, new { Message = "Ошибка при создании экзамена" });
        }
    }

    /// <summary>
    /// Обновить экзамен
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateExam(int id, [FromBody] UpdateExamDto dto)
    {
        try
        {
            var userId = await GetUserId();
            var isAdmin = User.IsInRole("Admin");
            var exam = await _examService.UpdateExamAsync(id, userId, dto, isAdmin);
            
            return Ok(new ExamDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                Difficulty = exam.Difficulty,
                Subjects = exam.Subjects?.Select(s => new SubjectDto { Id = s.Id, Name = s.Name }).ToList() ?? new List<SubjectDto>(),
                SubjectIds = exam.Subjects?.Select(s => s.Id).ToList() ?? new List<int>(),
                CountryId = exam.CountryId,
                UniversityId = exam.UniversityId,
                ExamTypeId = exam.ExamTypeId,
                MaxAttempts = exam.MaxAttempts,
                PassingScore = exam.PassingScore,
                IsProctored = exam.IsProctored,
                ShuffleQuestions = exam.ShuffleQuestions,
                ShuffleAnswers = exam.ShuffleAnswers,
                ShowResultsAfter = exam.ShowResultsAfter,
                ShowCorrectAnswers = exam.ShowCorrectAnswers,
                ShowDetailedFeedback = exam.ShowDetailedFeedback,
                TimeLimit = exam.TimeLimit,
                StrictTiming = exam.StrictTiming,
                IsPublished = exam.IsPublished,
                IsPublic = exam.IsPublic,
                MaxScore = exam.MaxScore,
                QuestionCount = exam.Questions?.Count ?? 0,
                CreatedAt = exam.CreatedAt,
                UserId = exam.UserId,
                UserName = exam.User?.UserName ?? "Unknown",
                Questions = exam.Questions?.OrderBy(q => q.OrderIndex).Select(q => new ExamQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Explanation = q.Explanation,
                    QuestionType = q.QuestionType,
                    Points = q.Points,
                    OrderIndex = q.OrderIndex,
                    Answers = q.Answers.OrderBy(a => a.OrderIndex).Select(a => new ExamAnswerDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        OrderIndex = a.OrderIndex
                    }).ToList()
                }).ToList() ?? new List<ExamQuestionDto>()
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exam {ExamId}", id);
            return StatusCode(500, new { Message = "Ошибка при обновлении экзамена" });
        }
    }

    /// <summary>
    /// Опубликовать экзамен
    /// </summary>
    [HttpPatch("{id}/publish")]
    public async Task<ActionResult> PublishExam(int id)
    {
        try
        {
            var userId = await GetUserId();
            var isAdmin = User.IsInRole("Admin");
            var result = await _examService.PublishExamAsync(id, userId, isAdmin);
            
            if (!result)
                return NotFound(new { Message = "Экзамен не найден или у вас нет прав" });

            return Ok(new { Message = "Экзамен опубликован", IsPublished = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing exam {ExamId}", id);
            return StatusCode(500, new { Message = "Ошибка при публикации экзамена" });
        }
    }

    /// <summary>
    /// Снять экзамен с публикации
    /// </summary>
    [HttpPatch("{id}/unpublish")]
    public async Task<ActionResult> UnpublishExam(int id)
    {
        try
        {
            var userId = await GetUserId();
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
                return NotFound(new { Message = "Экзамен не найден" });

            if (exam.UserId != userId && !userRoles.Contains("Admin"))
                return Forbid();

            exam.IsPublished = false;
            exam.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Экзамен снят с публикации", IsPublished = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing exam {ExamId}", id);
            return StatusCode(500, new { Message = "Ошибка при снятии экзамена с публикации" });
        }
    }

    /// <summary>
    /// Удалить экзамен
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExam(int id)
    {
        try
        {
            var userId = await GetUserId();
            var isAdmin = User.IsInRole("Admin");
            var result = await _examService.DeleteExamAsync(id, userId, isAdmin);
            
            if (!result)
                return NotFound(new { Message = "Экзамен не найден или у вас нет прав" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exam {ExamId}", id);
            return StatusCode(500, new { Message = "Ошибка при удалении экзамена" });
        }
    }
}
