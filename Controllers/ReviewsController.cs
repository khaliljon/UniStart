using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ReviewsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    // ============ QUIZ REVIEWS ============

    /// <summary>
    /// Получить все отзывы о квизе
    /// </summary>
    [HttpGet("quizzes/{quizId}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetQuizReviews(int quizId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null)
            return NotFound(new { Message = "Квиз не найден" });

        var reviews = await _context.QuizReviews
            .Include(r => r.User)
            .Where(r => r.QuizId == quizId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                User = new
                {
                    r.User.Id,
                    r.User.UserName,
                    r.User.Email
                }
            })
            .ToListAsync();

        var avgRating = await _context.QuizReviews
            .Where(r => r.QuizId == quizId)
            .AverageAsync(r => (double?)r.Rating) ?? 0;

        var totalReviews = await _context.QuizReviews.CountAsync(r => r.QuizId == quizId);

        return Ok(new
        {
            QuizId = quizId,
            AverageRating = Math.Round(avgRating, 2),
            TotalReviews = totalReviews,
            Reviews = reviews
        });
    }

    /// <summary>
    /// Добавить отзыв о квизе
    /// </summary>
    [HttpPost("quizzes/{quizId}")]
    public async Task<ActionResult<QuizReview>> AddQuizReview(int quizId, [FromBody] CreateReviewDto dto)
    {
        var userId = GetUserId();

        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null)
            return NotFound(new { Message = "Квиз не найден" });

        // Проверяем, не оставлял ли пользователь уже отзыв
        var existingReview = await _context.QuizReviews
            .FirstOrDefaultAsync(r => r.QuizId == quizId && r.UserId == userId);

        if (existingReview != null)
            return BadRequest(new { Message = "Вы уже оставили отзыв на этот квиз" });

        var review = new QuizReview
        {
            QuizId = quizId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.QuizReviews.Add(review);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пользователь {UserId} оставил отзыв на квиз {QuizId}", userId, quizId);

        return CreatedAtAction(nameof(GetQuizReviews), new { quizId }, review);
    }

    /// <summary>
    /// Обновить свой отзыв о квизе
    /// </summary>
    [HttpPut("quizzes/{quizId}")]
    public async Task<ActionResult> UpdateQuizReview(int quizId, [FromBody] CreateReviewDto dto)
    {
        var userId = GetUserId();

        var review = await _context.QuizReviews
            .FirstOrDefaultAsync(r => r.QuizId == quizId && r.UserId == userId);

        if (review == null)
            return NotFound(new { Message = "Отзыв не найден" });

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Отзыв обновлён" });
    }

    /// <summary>
    /// Удалить свой отзыв о квизе
    /// </summary>
    [HttpDelete("quizzes/{quizId}")]
    public async Task<ActionResult> DeleteQuizReview(int quizId)
    {
        var userId = GetUserId();

        var review = await _context.QuizReviews
            .FirstOrDefaultAsync(r => r.QuizId == quizId && r.UserId == userId);

        if (review == null)
            return NotFound(new { Message = "Отзыв не найден" });

        _context.QuizReviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ============ FLASHCARD SET REVIEWS ============

    /// <summary>
    /// Получить все отзывы о наборе карточек
    /// </summary>
    [HttpGet("flashcard-sets/{setId}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetFlashcardSetReviews(int setId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var flashcardSet = await _context.FlashcardSets.FindAsync(setId);
        if (flashcardSet == null)
            return NotFound(new { Message = "Набор карточек не найден" });

        var reviews = await _context.FlashcardSetReviews
            .Include(r => r.User)
            .Where(r => r.FlashcardSetId == setId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                User = new
                {
                    r.User.Id,
                    r.User.UserName,
                    r.User.Email
                }
            })
            .ToListAsync();

        var avgRating = await _context.FlashcardSetReviews
            .Where(r => r.FlashcardSetId == setId)
            .AverageAsync(r => (double?)r.Rating) ?? 0;

        var totalReviews = await _context.FlashcardSetReviews.CountAsync(r => r.FlashcardSetId == setId);

        return Ok(new
        {
            FlashcardSetId = setId,
            AverageRating = Math.Round(avgRating, 2),
            TotalReviews = totalReviews,
            Reviews = reviews
        });
    }

    /// <summary>
    /// Добавить отзыв о наборе карточек
    /// </summary>
    [HttpPost("flashcard-sets/{setId}")]
    public async Task<ActionResult<FlashcardSetReview>> AddFlashcardSetReview(int setId, [FromBody] CreateReviewDto dto)
    {
        var userId = GetUserId();

        var flashcardSet = await _context.FlashcardSets.FindAsync(setId);
        if (flashcardSet == null)
            return NotFound(new { Message = "Набор карточек не найден" });

        var existingReview = await _context.FlashcardSetReviews
            .FirstOrDefaultAsync(r => r.FlashcardSetId == setId && r.UserId == userId);

        if (existingReview != null)
            return BadRequest(new { Message = "Вы уже оставили отзыв на этот набор" });

        var review = new FlashcardSetReview
        {
            FlashcardSetId = setId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.FlashcardSetReviews.Add(review);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пользователь {UserId} оставил отзыв на набор карточек {SetId}", userId, setId);

        return CreatedAtAction(nameof(GetFlashcardSetReviews), new { setId }, review);
    }

    /// <summary>
    /// Обновить свой отзыв о наборе карточек
    /// </summary>
    [HttpPut("flashcard-sets/{setId}")]
    public async Task<ActionResult> UpdateFlashcardSetReview(int setId, [FromBody] CreateReviewDto dto)
    {
        var userId = GetUserId();

        var review = await _context.FlashcardSetReviews
            .FirstOrDefaultAsync(r => r.FlashcardSetId == setId && r.UserId == userId);

        if (review == null)
            return NotFound(new { Message = "Отзыв не найден" });

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Отзыв обновлён" });
    }

    /// <summary>
    /// Удалить свой отзыв о наборе карточек
    /// </summary>
    [HttpDelete("flashcard-sets/{setId}")]
    public async Task<ActionResult> DeleteFlashcardSetReview(int setId)
    {
        var userId = GetUserId();

        var review = await _context.FlashcardSetReviews
            .FirstOrDefaultAsync(r => r.FlashcardSetId == setId && r.UserId == userId);

        if (review == null)
            return NotFound(new { Message = "Отзыв не найден" });

        _context.FlashcardSetReviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs
public class CreateReviewDto
{
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [StringLength(1000)]
    public string? Comment { get; set; }
}
