using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Services;

namespace UniStart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlashcardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpacedRepetitionService _spacedRepetitionService;

        public FlashcardsController(
            ApplicationDbContext context,
            ISpacedRepetitionService spacedRepetitionService)
        {
            _context = context;
            _spacedRepetitionService = spacedRepetitionService;
        }

        // ============ FLASHCARD SETS ============

        /// <summary>
        /// Получить все наборы карточек
        /// </summary>
        [HttpGet("sets")]
        public async Task<ActionResult<List<FlashcardSetDto>>> GetFlashcardSets()
        {
            var sets = await _context.FlashcardSets
                .Include(fs => fs.Flashcards)
                .Select(fs => new FlashcardSetDto
                {
                    Id = fs.Id,
                    Title = fs.Title,
                    Description = fs.Description,
                    CreatedAt = fs.CreatedAt,
                    UpdatedAt = fs.UpdatedAt,
                    TotalCards = fs.Flashcards.Count,
                    CardsToReview = fs.Flashcards.Count(f => f.NextReviewDate == null || f.NextReviewDate <= DateTime.UtcNow)
                })
                .ToListAsync();

            return Ok(sets);
        }

        /// <summary>
        /// Получить набор по ID с карточками
        /// </summary>
        [HttpGet("sets/{id}")]
        public async Task<ActionResult<FlashcardSet>> GetFlashcardSet(int id)
        {
            var set = await _context.FlashcardSets
                .Include(fs => fs.Flashcards)
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (set == null)
                return NotFound($"FlashcardSet with ID {id} not found");

            return Ok(set);
        }

        /// <summary>
        /// Создать новый набор карточек
        /// </summary>
        [HttpPost("sets")]
        public async Task<ActionResult<FlashcardSet>> CreateFlashcardSet(CreateFlashcardSetDto dto)
        {
            var set = new FlashcardSet
            {
                Title = dto.Title,
                Description = dto.Description
            };

            _context.FlashcardSets.Add(set);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFlashcardSet), new { id = set.Id }, set);
        }

        /// <summary>
        /// Обновить набор карточек
        /// </summary>
        [HttpPut("sets/{id}")]
        public async Task<IActionResult> UpdateFlashcardSet(int id, UpdateFlashcardSetDto dto)
        {
            var set = await _context.FlashcardSets.FindAsync(id);
            if (set == null)
                return NotFound();

            set.Title = dto.Title;
            set.Description = dto.Description;
            set.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить набор карточек
        /// </summary>
        [HttpDelete("sets/{id}")]
        public async Task<IActionResult> DeleteFlashcardSet(int id)
        {
            var set = await _context.FlashcardSets.FindAsync(id);
            if (set == null)
                return NotFound();

            _context.FlashcardSets.Remove(set);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============ FLASHCARDS ============

        /// <summary>
        /// Получить карточки для повторения из набора
        /// </summary>
        [HttpGet("sets/{setId}/review")]
        public async Task<ActionResult<List<FlashcardDto>>> GetCardsForReview(int setId)
        {
            var cards = await _context.Flashcards
                .Where(f => f.FlashcardSetId == setId)
                .Where(f => f.NextReviewDate == null || f.NextReviewDate <= DateTime.UtcNow)
                .Select(f => new FlashcardDto
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer,
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
        /// Создать новую карточку
        /// </summary>
        [HttpPost("cards")]
        public async Task<ActionResult<Flashcard>> CreateFlashcard(CreateFlashcardDto dto)
        {
            // Проверяем существование набора
            var setExists = await _context.FlashcardSets.AnyAsync(fs => fs.Id == dto.FlashcardSetId);
            if (!setExists)
                return BadRequest("FlashcardSet not found");

            var flashcard = new Flashcard
            {
                Question = dto.Question,
                Answer = dto.Answer,
                Explanation = dto.Explanation,
                FlashcardSetId = dto.FlashcardSetId,
                OrderIndex = await _context.Flashcards
                    .Where(f => f.FlashcardSetId == dto.FlashcardSetId)
                    .CountAsync()
            };

            _context.Flashcards.Add(flashcard);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFlashcard), new { id = flashcard.Id }, flashcard);
        }

        /// <summary>
        /// Получить карточку по ID
        /// </summary>
        [HttpGet("cards/{id}")]
        public async Task<ActionResult<Flashcard>> GetFlashcard(int id)
        {
            var card = await _context.Flashcards.FindAsync(id);
            if (card == null)
                return NotFound();

            return Ok(card);
        }

        /// <summary>
        /// Обновить карточку
        /// </summary>
        [HttpPut("cards/{id}")]
        public async Task<IActionResult> UpdateFlashcard(int id, UpdateFlashcardDto dto)
        {
            var card = await _context.Flashcards.FindAsync(id);
            if (card == null)
                return NotFound();

            card.Question = dto.Question;
            card.Answer = dto.Answer;
            card.Explanation = dto.Explanation;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить карточку
        /// </summary>
        [HttpDelete("cards/{id}")]
        public async Task<IActionResult> DeleteFlashcard(int id)
        {
            var card = await _context.Flashcards.FindAsync(id);
            if (card == null)
                return NotFound();

            _context.Flashcards.Remove(card);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Отметить повторение карточки (Spaced Repetition)
        /// </summary>
        [HttpPost("cards/review")]
        public async Task<ActionResult<ReviewResultDto>> ReviewFlashcard(ReviewFlashcardDto dto)
        {
            var card = await _context.Flashcards.FindAsync(dto.FlashcardId);
            if (card == null)
                return NotFound("Flashcard not found");

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
