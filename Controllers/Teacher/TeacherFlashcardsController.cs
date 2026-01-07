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

namespace UniStart.Controllers.Teacher;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
public class TeacherFlashcardsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeacherFlashcardsController> _logger;

    public TeacherFlashcardsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TeacherFlashcardsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Создать приватный набор карточек (доступен только преподавателю)
    /// </summary>
    [HttpPost("flashcard-sets/private")]
    public async Task<ActionResult<FlashcardSetDto>> CreatePrivateFlashcardSet([FromBody] CreateFlashcardSetDto dto)
    {
        var userId = GetUserId();

        var flashcardSet = new FlashcardSet
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            IsPublic = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FlashcardSets.Add(flashcardSet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Преподаватель создал приватный набор карточек: {Title}", flashcardSet.Title);

        return CreatedAtAction("GetFlashcardSet", "Flashcards", new { id = flashcardSet.Id }, new FlashcardSetDto
        {
            Id = flashcardSet.Id,
            Title = flashcardSet.Title,
            Description = flashcardSet.Description,
            Subject = flashcardSet.Subject,
            IsPublic = flashcardSet.IsPublic,
            CreatedAt = flashcardSet.CreatedAt,
            UpdatedAt = flashcardSet.UpdatedAt,
            CardCount = 0,
            TotalCards = 0,
            CardsToReview = 0
        });
    }

    /// <summary>
    /// Получить все свои наборы карточек (публичные и приватные)
    /// </summary>
    [HttpGet("flashcard-sets/my")]
    public async Task<ActionResult<IEnumerable<FlashcardSetDto>>> GetMyFlashcardSets(
        [FromQuery] bool? isPublic = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var query = _context.FlashcardSets
            .Include(fs => fs.Flashcards)
            .Where(fs => fs.UserId == userId);

        if (isPublic.HasValue)
            query = query.Where(fs => fs.IsPublic == isPublic.Value);

        var flashcardSets = await query
            .OrderByDescending(fs => fs.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(fs => new FlashcardSetDto
            {
                Id = fs.Id,
                Title = fs.Title,
                Description = fs.Description,
                Subject = fs.Subject,
                IsPublic = fs.IsPublic,
                CreatedAt = fs.CreatedAt,
                UpdatedAt = fs.UpdatedAt,
                CardCount = fs.Flashcards.Count,
                TotalCards = fs.Flashcards.Count,
                CardsToReview = fs.Flashcards.Count(f => f.NextReviewDate == null || f.NextReviewDate <= DateTime.UtcNow)
            })
            .ToListAsync();

        return Ok(flashcardSets);
    }
}
