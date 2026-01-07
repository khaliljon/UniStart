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
using UniStart.Services;

namespace UniStart.Controllers.Flashcards
{
    [ApiController]
    [Route("api/flashcards")]
    [Authorize]
    public class FlashcardsSetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpacedRepetitionService _spacedRepetitionService;
        private readonly ILogger<FlashcardsSetsController> _logger;

        public FlashcardsSetsController(
            ApplicationDbContext context,
            ISpacedRepetitionService spacedRepetitionService,
            ILogger<FlashcardsSetsController> logger)
        {
            _context = context;
            _spacedRepetitionService = spacedRepetitionService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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

            var sets = await query.ToListAsync();
            
            // ИСПРАВЛЕНО: Получаем прогресс пользователя для расчета CardsToReview
            var setIds = sets.Select(s => s.Id).ToList();
            var allFlashcards = await _context.Flashcards
                .Where(f => setIds.Contains(f.FlashcardSetId))
                .Select(f => new { f.Id, f.FlashcardSetId })
                .ToListAsync();
            
            var flashcardIds = allFlashcards.Select(f => f.Id).ToList();
            var userProgress = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && flashcardIds.Contains(p.FlashcardId))
                .ToDictionaryAsync(p => p.FlashcardId);
            
            var now = DateTime.UtcNow;
            var result = sets.Select(fs => 
            {
                var setFlashcards = allFlashcards.Where(f => f.FlashcardSetId == fs.Id).ToList();
                var cardsToReview = setFlashcards.Count(f => 
                {
                    if (!userProgress.TryGetValue(f.Id, out var progress))
                        return true; // Новая карточка - нужно повторить
                    
                    return progress.NextReviewDate == null || progress.NextReviewDate <= now;
                });
                
                return new FlashcardSetDto
                {
                    Id = fs.Id,
                    Title = fs.Title,
                    Description = fs.Description,
                    Subject = fs.Subject,
                    IsPublic = fs.IsPublic,
                    IsPublished = fs.IsPublished,
                    CreatedAt = fs.CreatedAt,
                    UpdatedAt = fs.UpdatedAt,
                    CardCount = setFlashcards.Count,
                    TotalCards = setFlashcards.Count,
                    CardsToReview = cardsToReview // ИСПРАВЛЕНО: используем UserFlashcardProgress
                };
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Получить набор по ID с карточками (свои или публичные)
        /// При открытии набора создается/обновляется запись UserFlashcardSetAccess
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

            // Создаем или обновляем запись о доступе к набору
            var access = await _context.UserFlashcardSetAccesses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.FlashcardSetId == id);

            var now = DateTime.UtcNow;
            if (access == null)
            {
                // Первое открытие набора
                access = new UserFlashcardSetAccess
                {
                    UserId = userId,
                    FlashcardSetId = id,
                    FirstAccessedAt = now,
                    LastAccessedAt = now,
                    AccessCount = 1,
                    TotalCardsCount = set.Flashcards.Count,
                    CreatedAt = now
                };
                _context.UserFlashcardSetAccesses.Add(access);
            }
            else
            {
                // Обновляем информацию о доступе
                access.LastAccessedAt = now;
                access.AccessCount++;
                access.TotalCardsCount = set.Flashcards.Count; // Обновляем количество карточек на случай изменения
                access.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

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

        /// <summary>
        /// Опубликовать набор карточек (только свои)
        /// </summary>
        [HttpPatch("sets/{id}/publish")]
        public async Task<IActionResult> PublishFlashcardSet(int id)
        {
            var userId = GetUserId();
            
            var set = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.Id == id && fs.UserId == userId);
                
            if (set == null)
                return NotFound();

            set.IsPublished = true;
            set.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        /// <summary>
        /// Снять набор карточек с публикации (только свои)
        /// </summary>
        [HttpPatch("sets/{id}/unpublish")]
        public async Task<IActionResult> UnpublishFlashcardSet(int id)
        {
            var userId = GetUserId();
            
            var set = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.Id == id && fs.UserId == userId);
                
            if (set == null)
                return NotFound();

            set.IsPublished = false;
            set.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        /// <summary>
        /// Получить статистику по набору карточек
        /// </summary>
        [HttpGet("sets/{id}/stats")]
        public async Task<ActionResult> GetFlashcardSetStats(int id)
        {
            var userId = GetUserId();

            var set = await _context.FlashcardSets
                .Include(fs => fs.Flashcards)
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (set == null)
                return NotFound($"FlashcardSet with ID {id} not found");

            // Проверка доступа: только владелец набора может видеть статистику
            if (set.UserId != userId)
                return Forbid("Access denied: you can only view stats for your own flashcard sets");

            // Количество уникальных пользователей, изучающих этот набор (хотя бы раз открыли)
            // Исключаем владельца набора из подсчета студентов
            var uniqueStudentsCount = await _context.UserFlashcardSetAccesses
                .Where(a => a.FlashcardSetId == id && a.UserId != set.UserId)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            // Общее количество карточек в наборе
            var totalCards = set.Flashcards.Count;

            // Средний прогресс по всем пользователям (процент полностью изученных наборов)
            var completedSetsCount = await _context.UserFlashcardSetAccesses
                .Where(a => a.FlashcardSetId == id && a.IsCompleted)
                .CountAsync();

            var averageProgress = uniqueStudentsCount > 0
                ? Math.Round((double)completedSetsCount / uniqueStudentsCount * 100, 2)
                : 0.0;

            return Ok(new
            {
                set.Id,
                set.Title,
                set.Description,
                set.Subject,
                set.IsPublic,
                set.CreatedAt,
                set.UpdatedAt,
                TotalCards = totalCards,
                UniqueStudents = uniqueStudentsCount, // Изучающих студентов (хотя бы раз открыли набор)
                AverageProgress = averageProgress, // Средний процент завершенных наборов
                CompletedSetsCount = completedSetsCount // Количество пользователей, которые полностью изучили набор
            });
        }
    }
}
