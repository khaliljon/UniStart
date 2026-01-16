using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
using UniStart.Services;

namespace UniStart.Controllers.Exams;

/// <summary>
/// Контроллер для получения информации об экзаменах (GET запросы)
/// </summary>
[ApiController]
[Route("api/exams")]
public class ExamsQueryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IExamService _examService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ExamsQueryController> _logger;

    public ExamsQueryController(
        ApplicationDbContext context,
        IExamService examService,
        UserManager<ApplicationUser> userManager,
        ILogger<ExamsQueryController> logger)
    {
        _context = context;
        _examService = examService;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<string> GetUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? throw new UnauthorizedAccessException("Пользователь не авторизован");
    }

    /// <summary>
    /// Получить все опубликованные экзамены (для студентов)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ExamDto>>> GetAllExams(
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new ExamFilterDto
        {
            Search = search,
            Subject = subject,
            Difficulty = difficulty,
            Page = page,
            PageSize = pageSize
        };

        var result = await _examService.SearchExamsAsync(filter, onlyPublished: true);
        return Ok(result);
    }

    /// <summary>
    /// Получить свои экзамены (для преподавателей и админов)
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<PagedResult<ExamDto>>> GetMyExams(
        [FromQuery] string? search = null,
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = await GetUserId();

        var filter = new ExamFilterDto
        {
            Search = search,
            Subject = subject,
            Difficulty = difficulty,
            Page = page,
            PageSize = pageSize
        };

        var result = await _examService.GetMyExamsAsync(userId, filter);
        return Ok(result);
    }

    /// <summary>
    /// Получить экзамен по ID (детали)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExamDto>> GetExamById(int id)
    {
        var userId = User.Identity?.IsAuthenticated == true ? await GetUserId() : null;
        List<string> userRoles = new List<string>();
        if (User.Identity?.IsAuthenticated == true) 
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
                userRoles = (await _userManager.GetRolesAsync(user)).ToList();
        }
        var isAdmin = userRoles.Contains("Admin") || userRoles.Contains("Teacher");

        var examDto = await _examService.GetExamDetailAsync(id, userId, isAdmin);
        
        if (examDto == null)
            return NotFound("Экзамен не найден");

        return Ok(examDto);
    }

    /// <summary>
    /// Получить экзамен для прохождения (без правильных ответов)
    /// </summary>
    [HttpGet("{id}/take")]
    [Authorize]
    public async Task<ActionResult<ExamTakingDto>> GetExamForTaking(int id)
    {
        var userId = await GetUserId();

        var exam = await _context.Exams
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Attempts.Where(a => a.UserId == userId))
            .FirstOrDefaultAsync(t => t.Id == id && t.IsPublished);

        if (exam == null)
            return NotFound("Экзамен не найден или не опубликован");

        // Проверка количества попыток
        var attemptsCount = exam.Attempts.Count(a => a.CompletedAt != null);
        if (attemptsCount >= exam.MaxAttempts)
            return BadRequest($"Вы использовали все попытки ({exam.MaxAttempts})");

        var questions = exam.Questions.OrderBy(q => q.Order).ToList();

        if (exam.ShuffleQuestions)
            questions = questions.OrderBy(_ => Guid.NewGuid()).ToList();

        var examDto = new ExamTakingDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            TimeLimit = exam.TimeLimit,
            TotalPoints = exam.TotalPoints,
            PassingScore = exam.PassingScore,
            RemainingAttempts = exam.MaxAttempts - attemptsCount,
            Questions = questions.Select(q => new ExamQuestionTakingDto
            {
                Id = q.Id,
                Text = q.Text,
                QuestionType = q.QuestionType,
                Points = q.Points,
                Order = q.Order,
                Answers = (exam.ShuffleAnswers
                    ? q.Answers.OrderBy(_ => Guid.NewGuid())
                    : q.Answers.OrderBy(a => a.Order)
                ).Select(a => new ExamAnswerTakingDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    Order = a.Order
                }).ToList()
            }).ToList()
        };

        return Ok(examDto);
    }
}
