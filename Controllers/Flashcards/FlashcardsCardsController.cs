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

namespace UniStart.Controllers.Flashcards
{
    [ApiController]
    [Route("api/flashcards")]
    [Authorize]
    public class FlashcardsCardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlashcardsCardsController> _logger;

        public FlashcardsCardsController(
            ApplicationDbContext context,
            ILogger<FlashcardsCardsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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
            var isAdmin = User.IsInRole("Admin");
            
            var query = _context.Flashcards
                .Include(f => f.FlashcardSet)
                .Where(f => f.Id == id);
            
            // Админ может редактировать любые карточки, остальные - только свои
            if (!isAdmin)
                query = query.Where(f => f.FlashcardSet.UserId == userId);
            
            var card = await query.FirstOrDefaultAsync();
                
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
            var isAdmin = User.IsInRole("Admin");
            
            var query = _context.Flashcards
                .Include(f => f.FlashcardSet)
                .Where(f => f.Id == id);
            
            // Админ может удалять любые карточки, остальные - только свои
            if (!isAdmin)
                query = query.Where(f => f.FlashcardSet.UserId == userId);
            
            var card = await query.FirstOrDefaultAsync();
                
            if (card == null)
                return NotFound();

            _context.Flashcards.Remove(card);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
