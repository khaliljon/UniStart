using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniStart.DTOs;
using UniStart.Services;

namespace UniStart.Controllers.Quizzes;

/// <summary>
/// Контроллер для получения информации о квизах (GET запросы)
/// </summary>
[ApiController]
[Route("api/quizzes")]
public class QuizzesQueryController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizzesQueryController> _logger;

    public QuizzesQueryController(
        IQuizService quizService,
        ILogger<QuizzesQueryController> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Получить все опубликованные тесты (публичный доступ) с поиском и фильтрацией
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<QuizDto>>> GetQuizzes(
        [FromQuery] string? search = null,
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new QuizFilterDto
        {
            Search = search,
            Subject = subject,
            Difficulty = difficulty,
            Page = page,
            PageSize = pageSize
        };

        var result = await _quizService.SearchQuizzesAsync(filter, onlyPublished: true);
        return Ok(result);
    }

    /// <summary>
    /// Получить только свои тесты (все, включая неопубликованные) с поиском и фильтрацией
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<PagedResult<QuizDto>>> GetMyQuizzes(
        [FromQuery] string? search = null,
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId()!;
        
        var filter = new QuizFilterDto
        {
            Search = search,
            Subject = subject,
            Difficulty = difficulty,
            Page = page,
            PageSize = pageSize
        };

        var result = await _quizService.GetMyQuizzesAsync(userId, filter);
        return Ok(result);
    }

    /// <summary>
    /// Получить тест по ID с вопросами (БЕЗ правильных ответов для прохождения)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<QuizDetailDto>> GetQuiz(int id)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Teacher");
        
        var quizDto = await _quizService.GetQuizDetailAsync(id, userId, isAdmin);
        
        if (quizDto == null)
            return NotFound($"Quiz with ID {id} not found");

        return Ok(quizDto);
    }
}
