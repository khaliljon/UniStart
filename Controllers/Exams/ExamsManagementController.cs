using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;

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

    public ExamsManagementController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ExamsManagementController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
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
        var userId = await GetUserId();

        var exam = new Exam
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            Difficulty = dto.Difficulty,
            CountryId = dto.CountryId,
            UniversityId = dto.UniversityId,
            ExamTypeId = dto.ExamTypeId,
            MaxAttempts = dto.MaxAttempts,
            PassingScore = dto.PassingScore,
            IsProctored = dto.IsProctored,
            ShuffleQuestions = dto.ShuffleQuestions,
            ShuffleAnswers = dto.ShuffleAnswers,
            ShowResultsAfter = dto.ShowResultsAfter,
            ShowCorrectAnswers = dto.ShowCorrectAnswers,
            ShowDetailedFeedback = dto.ShowDetailedFeedback,
            TimeLimit = dto.TimeLimit,
            IsPublished = dto.IsPublished,
            IsPublic = dto.IsPublic,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TotalPoints = dto.Questions.Sum(q => q.Points)
        };

        foreach (var questionDto in dto.Questions)
        {
            var question = new ExamQuestion
            {
                Text = questionDto.Text,
                Explanation = questionDto.Explanation,
                Points = questionDto.Points,
                Order = questionDto.Order,
                Exam = exam
            };

            foreach (var answerDto in questionDto.Answers)
            {
                question.Answers.Add(new ExamAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    Order = answerDto.Order,
                    Question = question
                });
            }

            exam.Questions.Add(question);
        }

        if (dto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            exam.Tags = tags;
        }

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetExamById", "ExamsQuery", new { id = exam.Id }, new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Subject = exam.Subject,
            Difficulty = exam.Difficulty,
            MaxAttempts = exam.MaxAttempts,
            PassingScore = exam.PassingScore,
            IsPublished = exam.IsPublished,
            IsPublic = exam.IsPublic,
            TotalPoints = exam.TotalPoints,
            QuestionCount = exam.Questions.Count,
            CreatedAt = exam.CreatedAt,
            UserId = exam.UserId,
            Tags = exam.Tags.Select(t => t.Name).ToList()
        });
    }

    /// <summary>
    /// Обновить экзамен
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateExam(int id, [FromBody] CreateExamDto dto)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (exam == null)
            return NotFound("Экзамен не найден");

        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        exam.Title = dto.Title;
        exam.Description = dto.Description;
        exam.Subject = dto.Subject;
        exam.Difficulty = dto.Difficulty;
        exam.CountryId = dto.CountryId;
        exam.UniversityId = dto.UniversityId;
        exam.ExamTypeId = dto.ExamTypeId;
        exam.MaxAttempts = dto.MaxAttempts;
        exam.PassingScore = dto.PassingScore;
        exam.IsProctored = dto.IsProctored;
        exam.ShuffleQuestions = dto.ShuffleQuestions;
        exam.ShuffleAnswers = dto.ShuffleAnswers;
        exam.ShowResultsAfter = dto.ShowResultsAfter;
        exam.ShowCorrectAnswers = dto.ShowCorrectAnswers;
        exam.ShowDetailedFeedback = dto.ShowDetailedFeedback;
        exam.TimeLimit = dto.TimeLimit;
        exam.IsPublished = dto.IsPublished;
        exam.IsPublic = dto.IsPublic;
        exam.UpdatedAt = DateTime.UtcNow;
        exam.TotalPoints = dto.Questions.Sum(q => q.Points);

        _context.ExamQuestions.RemoveRange(exam.Questions);
        exam.Questions.Clear();

        foreach (var questionDto in dto.Questions)
        {
            var question = new ExamQuestion
            {
                Text = questionDto.Text,
                Explanation = questionDto.Explanation,
                QuestionType = questionDto.QuestionType,
                Points = questionDto.Points,
                Order = questionDto.Order,
                Exam = exam
            };

            foreach (var answerDto in questionDto.Answers)
            {
                question.Answers.Add(new ExamAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    Order = answerDto.Order,
                    Question = question
                });
            }

            exam.Questions.Add(question);
        }

        exam.Tags.Clear();
        if (dto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            exam.Tags = tags;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Опубликовать экзамен
    /// </summary>
    [HttpPatch("{id}/publish")]
    public async Task<ActionResult> PublishExam(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
            return NotFound("Экзамен не найден");

        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        exam.IsPublished = true;
        exam.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { isPublished = exam.IsPublished });
    }

    /// <summary>
    /// Снять экзамен с публикации
    /// </summary>
    [HttpPatch("{id}/unpublish")]
    public async Task<ActionResult> UnpublishExam(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
            return NotFound("Экзамен не найден");

        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        exam.IsPublished = false;
        exam.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { isPublished = exam.IsPublished });
    }

    /// <summary>
    /// Удалить экзамен
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExam(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var exam = await _context.Exams
            .Include(t => t.Questions)
            .Include(t => t.Attempts)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (exam == null)
            return NotFound("Экзамен не найден");

        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
