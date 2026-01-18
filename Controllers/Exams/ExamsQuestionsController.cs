using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

namespace UniStart.Controllers.Exams;

/// <summary>
/// Контроллер для управления вопросами и ответами экзаменов
/// </summary>
[ApiController]
[Route("api/exams")]
[Authorize(Roles = "Teacher,Admin")]
public class ExamsQuestionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ExamsQuestionsController> _logger;

    public ExamsQuestionsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ExamsQuestionsController> logger)
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

    // ============ QUESTIONS CRUD ============

    /// <summary>
    /// Создать новый вопрос для экзамена (только для владельца экзамена)
    /// </summary>
    [HttpPost("{examId}/questions")]
    public async Task<ActionResult<ExamQuestion>> CreateQuestion(int examId, CreateExamQuestionDto dto)
    {
        var userId = await GetUserId();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var exam = await _context.Exams
            .FirstOrDefaultAsync(e => e.Id == examId);
            
        if (exam == null)
            return NotFound("Exam not found");

        if (exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        var question = new ExamQuestion
        {
            Text = dto.Text,
            Explanation = dto.Explanation,
            QuestionType = dto.QuestionType,
            Points = dto.Points,
            ExamId = examId,
            OrderIndex = await _context.ExamQuestions
                .Where(q => q.ExamId == examId)
                .MaxAsync(q => (int?)q.OrderIndex) ?? 0 + 1
        };

        _context.ExamQuestions.Add(question);
        await _context.SaveChangesAsync();

        // Обновляем MaxScore экзамена
        exam.MaxScore = await _context.ExamQuestions
            .Where(q => q.ExamId == exam.Id)
            .SumAsync(q => q.Points);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
    }

    /// <summary>
    /// Получить вопрос по ID
    /// </summary>
    [HttpGet("questions/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExamQuestion>> GetQuestion(int id)
    {
        var question = await _context.ExamQuestions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return NotFound();

        return Ok(question);
    }

    /// <summary>
    /// Обновить вопрос (только для владельца экзамена)
    /// </summary>
    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, UpdateExamQuestionDto dto)
    {
        var userId = await GetUserId();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var question = await _context.ExamQuestions
            .Include(q => q.Exam)
            .FirstOrDefaultAsync(q => q.Id == id);
            
        if (question == null)
            return NotFound("Question not found");

        if (question.Exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        var oldPoints = question.Points;
        
        question.Text = dto.QuestionText;
        question.Explanation = dto.Explanation;
        question.QuestionType = dto.QuestionType;
        question.Points = dto.Points;

        await _context.SaveChangesAsync();

        // Обновляем MaxScore если изменились баллы
        if (oldPoints != dto.Points)
        {
            var exam = await _context.Exams.FindAsync(question.ExamId);
            if (exam != null)
            {
                exam.MaxScore = await _context.ExamQuestions
                    .Where(q => q.ExamId == exam.Id)
                    .SumAsync(q => q.Points);
                await _context.SaveChangesAsync();
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить вопрос (только для владельца экзамена)
    /// </summary>
    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var userId = await GetUserId();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var question = await _context.ExamQuestions
            .Include(q => q.Exam)
            .FirstOrDefaultAsync(q => q.Id == id);
            
        if (question == null)
            return NotFound("Question not found");

        if (question.Exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        var examId = question.ExamId;
        _context.ExamQuestions.Remove(question);
        await _context.SaveChangesAsync();

        // Обновляем MaxScore экзамена
        var exam = await _context.Exams.FindAsync(question.ExamId);
        if (exam != null)
        {
            exam.MaxScore = await _context.ExamQuestions
                .Where(q => q.ExamId == exam.Id)
                .SumAsync(q => q.Points);
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    // ============ ANSWERS CRUD ============

    /// <summary>
    /// Создать новый ответ для вопроса (только для владельца экзамена)
    /// </summary>
    [HttpPost("questions/{questionId}/answers")]
    public async Task<ActionResult<ExamAnswer>> CreateAnswer(int questionId, CreateExamAnswerDto dto)
    {
        var userId = await GetUserId();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var question = await _context.ExamQuestions
            .Include(q => q.Exam)
            .FirstOrDefaultAsync(q => q.Id == questionId);
            
        if (question == null)
            return NotFound("Question not found");

        if (question.Exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        var answer = new ExamAnswer
        {
            Text = dto.Text,
            IsCorrect = dto.IsCorrect,
            QuestionId = questionId,
            OrderIndex = await _context.ExamAnswers
                .Where(a => a.QuestionId == questionId)
                .MaxAsync(a => (int?)a.OrderIndex) ?? 0 + 1
        };

        _context.ExamAnswers.Add(answer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAnswer), new { id = answer.Id }, answer);
    }

    /// <summary>
    /// Получить ответ по ID
    /// </summary>
    [HttpGet("answers/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExamAnswer>> GetAnswer(int id)
    {
        var answer = await _context.ExamAnswers.FindAsync(id);

        if (answer == null)
            return NotFound();

        return Ok(answer);
    }

    /// <summary>
    /// Обновить ответ (только для владельца экзамена)
    /// </summary>
    [HttpPut("answers/{id}")]
    public async Task<IActionResult> UpdateAnswer(int id, UpdateExamAnswerDto dto)
    {
        var userId = await GetUserId();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var answer = await _context.ExamAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.Exam)
            .FirstOrDefaultAsync(a => a.Id == id);
            
        if (answer == null)
            return NotFound("Answer not found");

        if (answer.Question.Exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        answer.Text = dto.AnswerText;
        answer.IsCorrect = dto.IsCorrect;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить ответ (только для владельца экзамена)
    /// </summary>
    [HttpDelete("answers/{id}")]
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        var userId = await GetUserId();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var userRoles = await _userManager.GetRolesAsync(user);
        
        var answer = await _context.ExamAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.Exam)
            .FirstOrDefaultAsync(a => a.Id == id);
            
        if (answer == null)
            return NotFound("Answer not found");

        if (answer.Question.Exam.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        _context.ExamAnswers.Remove(answer);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
