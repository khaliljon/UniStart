using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Services;

namespace UniStart.Controllers.Quizzes;

/// <summary>
/// Контроллер для управления квизами (CREATE, UPDATE, DELETE, Publish)
/// </summary>
[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesManagementController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizzesManagementController> _logger;

    public QuizzesManagementController(
        ApplicationDbContext context,
        IQuizService quizService,
        ILogger<QuizzesManagementController> logger)
    {
        _context = context;
        _quizService = quizService;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Создать новый тест
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Quiz>> CreateQuiz(CreateQuizDto dto)
    {
        var userId = GetUserId()!;
        
        var quiz = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            TimeLimit = dto.TimeLimit,
            Subject = dto.Subject,
            Difficulty = dto.Difficulty,
            IsPublic = dto.IsPublic,
            IsPublished = dto.IsPublished,
            IsLearningMode = dto.IsLearningMode,
            UserId = userId
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetQuiz", "QuizzesQuery", new { id = quiz.Id }, quiz);
    }

    /// <summary>
    /// Обновить тест (только свои) включая вопросы и ответы
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuiz(int id, UpdateQuizDto dto)
    {
        var userId = GetUserId()!;
        
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qu => qu.Answers)
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);
            
        if (quiz == null)
            return NotFound();

        // Обновляем базовую информацию
        quiz.Title = dto.Title;
        quiz.Description = dto.Description;
        quiz.TimeLimit = dto.TimeLimit;
        quiz.Subject = dto.Subject;
        quiz.Difficulty = dto.Difficulty;
        quiz.IsPublic = dto.IsPublic;
        quiz.IsPublished = dto.IsPublished;
        quiz.UpdatedAt = DateTime.UtcNow;

        // Удаляем старые вопросы и ответы
        _context.QuizQuestions.RemoveRange(quiz.Questions);

        // Добавляем новые вопросы с ответами
        foreach (var questionDto in dto.Questions)
        {
            var QuizQuestion = new QuizQuestion
            {
                Text = questionDto.Text,
                Points = questionDto.Points,
                Explanation = questionDto.Explanation,
                OrderIndex = questionDto.Order,
                QuizId = quiz.Id
            };

            foreach (var answerDto in questionDto.Answers)
            {
                QuizQuestion.Answers.Add(new QuizAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    OrderIndex = answerDto.Order
                });
            }

            quiz.Questions.Add(QuizQuestion);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить тест (свои или любые для админа)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Teacher");
        
        var result = await _quizService.DeleteQuizAsync(id, userId, isAdmin);
        
        if (!result)
            return StatusCode(403, new { message = "У вас нет прав для удаления этого квиза" });

        return NoContent();
    }

    /// <summary>
    /// Опубликовать квиз
    /// </summary>
    [HttpPatch("{id}/publish")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> PublishQuiz(int id)
    {
        var userId = GetUserId()!;
        
        var result = await _quizService.PublishQuizAsync(id, userId);
        
        if (!result)
            return NotFound();

        return Ok(new { isPublished = true });
    }

    /// <summary>
    /// Снять квиз с публикации
    /// </summary>
    [HttpPatch("{id}/unpublish")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> UnpublishQuiz(int id)
    {
        var userId = GetUserId()!;
        var isAdmin = User.IsInRole("Admin");
        
        var result = await _quizService.UnpublishQuizAsync(id, userId, isAdmin);
        
        if (!result)
            return NotFound();

        return Ok(new { isPublished = false });
    }
}
