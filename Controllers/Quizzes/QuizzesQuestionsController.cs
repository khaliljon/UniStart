using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Controllers.Quizzes;

/// <summary>
/// Контроллер для управления вопросами и ответами квизов
/// </summary>
[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesQuestionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuizzesQuestionsController> _logger;

    public QuizzesQuestionsController(
        ApplicationDbContext context,
        ILogger<QuizzesQuestionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ============ QUESTIONS CRUD ============

    /// <summary>
    /// Создать новый вопрос для квиза (только для владельца квиза)
    /// </summary>
    [HttpPost("questions")]
    public async Task<ActionResult<QuizQuestion>> CreateQuestion(CreateQuizQuestionDto dto)
    {
        var userId = GetUserId()!;
        
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == dto.QuizId && q.UserId == userId);
            
        if (quiz == null)
            return NotFound("Quiz not found or access denied");

        var QuizQuestion = new QuizQuestion
        {
            Text = dto.Text,
            Points = dto.Points,
            ImageUrl = dto.ImageUrl,
            Explanation = dto.Explanation,
            QuizId = dto.QuizId,
            OrderIndex = await _context.QuizQuestions
                .Where(q => q.QuizId == dto.QuizId)
                .CountAsync()
        };

        _context.QuizQuestions.Add(QuizQuestion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuestion), new { id = QuizQuestion.Id }, QuizQuestion);
    }

    /// <summary>
    /// Получить вопрос по ID
    /// </summary>
    [HttpGet("questions/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<QuizQuestion>> GetQuestion(int id)
    {
        var QuizQuestion = await _context.QuizQuestions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (QuizQuestion == null)
            return NotFound();

        return Ok(QuizQuestion);
    }

    /// <summary>
    /// Обновить вопрос (только для владельца квиза)
    /// </summary>
    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, UpdateQuizQuestionDto dto)
    {
        var userId = GetUserId()!;
        
        var QuizQuestion = await _context.QuizQuestions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == userId);
            
        if (QuizQuestion == null)
            return NotFound("Question not found or access denied");

        QuizQuestion.Text = dto.Text;
        QuizQuestion.Points = dto.Points;
        QuizQuestion.ImageUrl = dto.ImageUrl;
        QuizQuestion.Explanation = dto.Explanation;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить вопрос (только для владельца квиза)
    /// </summary>
    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var userId = GetUserId()!;
        
        var QuizQuestion = await _context.QuizQuestions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == userId);
            
        if (QuizQuestion == null)
            return NotFound("Question not found or access denied");

        _context.QuizQuestions.Remove(QuizQuestion);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ============ ANSWERS CRUD ============

    /// <summary>
    /// Создать новый ответ для вопроса (только для владельца квиза)
    /// </summary>
    [HttpPost("answers")]
    public async Task<ActionResult<QuizAnswer>> CreateAnswer(CreateQuizAnswerDto dto)
    {
        var userId = GetUserId()!;
        
        var QuizQuestion = await _context.QuizQuestions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.Id == dto.QuestionId && q.Quiz.UserId == userId);
            
        if (QuizQuestion == null)
            return NotFound("Question not found or access denied");

        var QuizAnswer = new QuizAnswer
        {
            Text = dto.Text,
            IsCorrect = dto.IsCorrect,
            QuestionId = dto.QuestionId,
            OrderIndex = await _context.QuizAnswers
                .Where(a => a.QuestionId == dto.QuestionId)
                .CountAsync()
        };

        _context.QuizAnswers.Add(QuizAnswer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAnswer), new { id = QuizAnswer.Id }, QuizAnswer);
    }

    /// <summary>
    /// Получить ответ по ID
    /// </summary>
    [HttpGet("answers/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<QuizAnswer>> GetAnswer(int id)
    {
        var QuizAnswer = await _context.QuizAnswers.FindAsync(id);

        if (QuizAnswer == null)
            return NotFound();

        return Ok(QuizAnswer);
    }

    /// <summary>
    /// Обновить ответ (только для владельца квиза)
    /// </summary>
    [HttpPut("answers/{id}")]
    public async Task<IActionResult> UpdateAnswer(int id, UpdateQuizAnswerDto dto)
    {
        var userId = GetUserId()!;
        
        var QuizAnswer = await _context.QuizAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.Quiz)
            .FirstOrDefaultAsync(a => a.Id == id && a.Question.Quiz.UserId == userId);
            
        if (QuizAnswer == null)
            return NotFound("Answer not found or access denied");

        QuizAnswer.Text = dto.Text;
        QuizAnswer.IsCorrect = dto.IsCorrect;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить ответ (только для владельца квиза)
    /// </summary>
    [HttpDelete("answers/{id}")]
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        var userId = GetUserId()!;
        
        var QuizAnswer = await _context.QuizAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.Quiz)
            .FirstOrDefaultAsync(a => a.Id == id && a.Question.Quiz.UserId == userId);
            
        if (QuizAnswer == null)
            return NotFound("Answer not found or access denied");

        _context.QuizAnswers.Remove(QuizAnswer);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
