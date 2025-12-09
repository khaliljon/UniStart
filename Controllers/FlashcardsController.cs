using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Services;

namespace UniStart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FlashcardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpacedRepetitionService _spacedRepetitionService;
        private readonly ILogger<FlashcardsController> _logger;

        public FlashcardsController(
            ApplicationDbContext context,
            ISpacedRepetitionService spacedRepetitionService,
            ILogger<FlashcardsController> logger)
        {
            _context = context;
            _spacedRepetitionService = spacedRepetitionService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ============ FLASHCARD SETS ============

        /// <summary>
        /// Получить все наборы карточек: свои + публичные наборы других пользователей
        /// </summary>
        [HttpGet("sets")]
        public async Task<ActionResult<List<FlashcardSetDto>>> GetFlashcardSets(
            [FromQuery] string? search = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null,
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null)
        {
            var userId = GetUserId();
            
            // Получаем свои наборы + публичные наборы других пользователей
            var query = _context.FlashcardSets
                .Where(fs => fs.UserId == userId || fs.IsPublic)
                .Include(fs => fs.Flashcards)
                .AsQueryable();

            // Поиск по названию и описанию
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(fs => 
                    fs.Title.ToLower().Contains(search.ToLower()) ||
                    fs.Description.ToLower().Contains(search.ToLower()));
            }

            // Фильтр по дате создания
            if (createdAfter.HasValue)
            {
                query = query.Where(fs => fs.CreatedAt >= createdAfter.Value);
            }

            if (createdBefore.HasValue)
            {
                query = query.Where(fs => fs.CreatedAt <= createdBefore.Value);
            }

            // Пагинация
            if (page.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var sets = await query
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

            return Ok(sets);
        }

        /// <summary>
        /// Получить набор по ID с карточками (свои или публичные)
        /// </summary>
        [HttpGet("sets/{id}")]
        public async Task<ActionResult<FlashcardSet>> GetFlashcardSet(int id)
        {
            var userId = GetUserId();
            
            // Можно просматривать свои наборы или публичные наборы других пользователей
            var set = await _context.FlashcardSets
                .Where(fs => fs.UserId == userId || fs.IsPublic)
                .Include(fs => fs.Flashcards)
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (set == null)
                return NotFound($"FlashcardSet with ID {id} not found");

            _logger.LogInformation("GetFlashcardSet: ID={Id}, UserId={UserId}, FlashcardsCount={Count}", 
                id, userId, set.Flashcards.Count);

            return Ok(set);
        }

        /// <summary>
        /// Создать новый набор карточек
        /// </summary>
        [HttpPost("sets")]
        public async Task<ActionResult<FlashcardSet>> CreateFlashcardSet(CreateFlashcardSetDto dto)
        {
            var userId = GetUserId();
            
            var set = new FlashcardSet
            {
                Title = dto.Title,
                Description = dto.Description,
                Subject = dto.Subject,
                IsPublic = dto.IsPublic,
                UserId = userId
            };

            _context.FlashcardSets.Add(set);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFlashcardSet), new { id = set.Id }, set);
        }

        /// <summary>
        /// Обновить набор карточек (только свои)
        /// </summary>
        [HttpPut("sets/{id}")]
        public async Task<IActionResult> UpdateFlashcardSet(int id, UpdateFlashcardSetDto dto)
        {
            var userId = GetUserId();
            
            var set = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.Id == id && fs.UserId == userId);
                
            if (set == null)
                return NotFound();

            set.Title = dto.Title;
            set.Description = dto.Description;
            set.Subject = dto.Subject;
            set.IsPublic = dto.IsPublic;
            set.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить набор карточек (только свои)
        /// </summary>
        [HttpDelete("sets/{id}")]
        public async Task<IActionResult> DeleteFlashcardSet(int id)
        {
            var userId = GetUserId();
            
            var set = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.Id == id && fs.UserId == userId);
                
            if (set == null)
                return NotFound();

            _context.FlashcardSets.Remove(set);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============ FLASHCARDS ============

        /// <summary>
        /// Получить карточки для повторения из набора (свои или публичные)
        /// </summary>
        [HttpGet("sets/{setId}/review")]
        public async Task<ActionResult<List<FlashcardDto>>> GetCardsForReview(int setId)
        {
            var userId = GetUserId();
            
            // Проверяем, что набор существует и доступен (свой или публичный)
            var setExists = await _context.FlashcardSets
                .AnyAsync(fs => fs.Id == setId && (fs.UserId == userId || fs.IsPublic));
                
            if (!setExists)
                return NotFound("FlashcardSet not found or access denied");
            
            var cards = await _context.Flashcards
                .Where(f => f.FlashcardSetId == setId)
                .Where(f => f.NextReviewDate == null || f.NextReviewDate <= DateTime.UtcNow)
                .Select(f => new FlashcardDto
                {
                    Id = f.Id,
                    Type = f.Type,
                    Question = f.Question,
                    Answer = f.Answer,
                    OptionsJson = f.OptionsJson,
                    MatchingPairsJson = f.MatchingPairsJson,
                    SequenceJson = f.SequenceJson,
                    Explanation = f.Explanation,
                    OrderIndex = f.OrderIndex,
                    NextReviewDate = f.NextReviewDate,
                    LastReviewedAt = f.LastReviewedAt,
                    IsDueForReview = true
                })
                .ToListAsync();

            return Ok(cards);
        }

        /// <summary>
        /// Создать новую карточку (только в своих наборах)
        /// </summary>
        [HttpPost("cards")]
        public async Task<ActionResult<Flashcard>> CreateFlashcard(CreateFlashcardDto dto)
        {
            var userId = GetUserId();
            
            _logger.LogInformation("CreateFlashcard: FlashcardSetId={SetId}, UserId={UserId}", 
                dto.FlashcardSetId, userId);
            
            // Проверяем существование набора и принадлежность пользователю
            var setExists = await _context.FlashcardSets
                .AnyAsync(fs => fs.Id == dto.FlashcardSetId && fs.UserId == userId);
                
            if (!setExists)
            {
                _logger.LogWarning("CreateFlashcard: FlashcardSet not found or access denied. SetId={SetId}, UserId={UserId}", 
                    dto.FlashcardSetId, userId);
                return BadRequest("FlashcardSet not found or access denied");
            }

            var flashcard = new Flashcard
            {
                Type = dto.Type,
                Question = dto.Question,
                Answer = dto.Answer,
                OptionsJson = dto.OptionsJson,
                MatchingPairsJson = dto.MatchingPairsJson,
                SequenceJson = dto.SequenceJson,
                Explanation = dto.Explanation,
                FlashcardSetId = dto.FlashcardSetId,
                OrderIndex = await _context.Flashcards
                    .Where(f => f.FlashcardSetId == dto.FlashcardSetId)
                    .CountAsync()
            };

            _context.Flashcards.Add(flashcard);
            await _context.SaveChangesAsync();

            _logger.LogInformation("CreateFlashcard: Created flashcard ID={Id} in set {SetId}", 
                flashcard.Id, dto.FlashcardSetId);

            return CreatedAtAction(nameof(GetFlashcard), new { id = flashcard.Id }, flashcard);
        }

        /// <summary>
        /// Получить карточку по ID (свои или из публичных наборов)
        /// </summary>
        [HttpGet("cards/{id}")]
        public async Task<ActionResult<Flashcard>> GetFlashcard(int id)
        {
            var userId = GetUserId();
            
            var card = await _context.Flashcards
                .Include(f => f.FlashcardSet)
                .FirstOrDefaultAsync(f => f.Id == id && (f.FlashcardSet.UserId == userId || f.FlashcardSet.IsPublic));
                
            if (card == null)
                return NotFound();

            return Ok(card);
        }

        /// <summary>
        /// Обновить карточку (только свои)
        /// </summary>
        [HttpPut("cards/{id}")]
        public async Task<IActionResult> UpdateFlashcard(int id, UpdateFlashcardDto dto)
        {
            var userId = GetUserId();
            
            var card = await _context.Flashcards
                .Include(f => f.FlashcardSet)
                .FirstOrDefaultAsync(f => f.Id == id && f.FlashcardSet.UserId == userId);
                
            if (card == null)
                return NotFound();

            card.Type = dto.Type;
            card.Question = dto.Question;
            card.Answer = dto.Answer;
            card.OptionsJson = dto.OptionsJson;
            card.MatchingPairsJson = dto.MatchingPairsJson;
            card.SequenceJson = dto.SequenceJson;
            card.Explanation = dto.Explanation;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить карточку (только свои)
        /// </summary>
        [HttpDelete("cards/{id}")]
        public async Task<IActionResult> DeleteFlashcard(int id)
        {
            var userId = GetUserId();
            
            var card = await _context.Flashcards
                .Include(f => f.FlashcardSet)
                .FirstOrDefaultAsync(f => f.Id == id && f.FlashcardSet.UserId == userId);
                
            if (card == null)
                return NotFound();

            _context.Flashcards.Remove(card);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Отметить повторение карточки (Spaced Repetition) - свои или из публичных наборов
        /// </summary>
        [HttpPost("cards/review")]
        public async Task<ActionResult<ReviewResultDto>> ReviewFlashcard(ReviewFlashcardDto dto)
        {
            var userId = GetUserId();
            
            var card = await _context.Flashcards
                .Include(f => f.FlashcardSet)
                .FirstOrDefaultAsync(f => f.Id == dto.FlashcardId && (f.FlashcardSet.UserId == userId || f.FlashcardSet.IsPublic));
                
            if (card == null)
                return NotFound("Flashcard not found or access denied");

            if (dto.Quality < 0 || dto.Quality > 5)
                return BadRequest("Quality must be between 0 and 5");

            // Применяем алгоритм интервального повторения
            _spacedRepetitionService.UpdateFlashcard(card, dto.Quality);
            await _context.SaveChangesAsync();

            var result = new ReviewResultDto
            {
                FlashcardId = card.Id,
                NextReviewDate = card.NextReviewDate ?? DateTime.UtcNow,
                IntervalDays = card.Interval,
                Message = dto.Quality >= 3 
                    ? $"Отлично! Следующее повторение через {card.Interval} дн." 
                    : "Попробуй ещё раз!"
            };

            return Ok(result);
        }
    }
}
