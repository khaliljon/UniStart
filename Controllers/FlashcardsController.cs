using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<FlashcardsController> _logger;

        public FlashcardsController(
            ApplicationDbContext context,
            ISpacedRepetitionService spacedRepetitionService,
            UserManager<ApplicationUser> userManager,
            ILogger<FlashcardsController> logger)
        {
            _context = context;
            _spacedRepetitionService = spacedRepetitionService;
            _userManager = userManager;
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

        // ============ FLASHCARDS ============

        /// <summary>
        /// Получить карточки для повторения из набора (свои или публичные)
        /// Использует UserFlashcardProgress для определения карточек к повторению
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
            
            // Получаем все карточки набора
            var allCards = await _context.Flashcards
                .Where(f => f.FlashcardSetId == setId)
                .ToListAsync();

            // Получаем прогресс пользователя по этим карточкам
            var userProgress = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && allCards.Select(c => c.Id).Contains(p.FlashcardId))
                .ToDictionaryAsync(p => p.FlashcardId);

            var cardsForReview = new List<FlashcardDto>();
            var now = DateTime.UtcNow;

            foreach (var card in allCards)
            {
                bool isDueForReview = false;
                DateTime? nextReviewDate = null;

                if (userProgress.TryGetValue(card.Id, out var progress))
                {
                    // Используем прогресс пользователя
                    isDueForReview = _spacedRepetitionService.IsDueForReview(progress);
                    nextReviewDate = progress.NextReviewDate;
                }
                else
                {
                    // Карточка еще не изучалась пользователем - нужно повторить
                    isDueForReview = true;
                }

                if (isDueForReview)
                {
                    cardsForReview.Add(new FlashcardDto
                    {
                        Id = card.Id,
                        Type = card.Type,
                        Question = card.Question,
                        Answer = card.Answer,
                        OptionsJson = card.OptionsJson,
                        MatchingPairsJson = card.MatchingPairsJson,
                        SequenceJson = card.SequenceJson,
                        Explanation = card.Explanation,
                        OrderIndex = card.OrderIndex,
                        NextReviewDate = nextReviewDate,
                        LastReviewedAt = userProgress.TryGetValue(card.Id, out var p) ? p.LastReviewedAt : null,
                        IsDueForReview = true
                    });
                }
            }

            return Ok(cardsForReview);
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
            var uniqueStudentsCount = await _context.UserFlashcardSetAccesses
                .Where(a => a.FlashcardSetId == id)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            // Количество карточек к повторению для текущего пользователя (если владелец тоже изучает)
            var cardsToReviewForUser = 0;
            if (set.UserId == userId)
            {
                var userProgress = await _context.UserFlashcardProgresses
                    .Where(p => p.UserId == userId && p.Flashcard.FlashcardSetId == id)
                    .ToListAsync();

                var allCards = await _context.Flashcards
                    .Where(f => f.FlashcardSetId == id)
                    .Select(f => f.Id)
                    .ToListAsync();

                cardsToReviewForUser = allCards.Count(cardId =>
                {
                    var progress = userProgress.FirstOrDefault(p => p.FlashcardId == cardId);
                    if (progress == null) return true; // Карточка еще не изучалась
                    return _spacedRepetitionService.IsDueForReview(progress);
                });
            }

            // Общее количество карточек в наборе
            var totalCards = set.Flashcards.Count;

            // Средний прогресс по всем пользователям (процент полностью изученных наборов)
            var completedSetsCount = await _context.UserFlashcardSetAccesses
                .Where(a => a.FlashcardSetId == id && a.IsCompleted)
                .CountAsync();

            var averageProgress = uniqueStudentsCount > 0
                ? Math.Round((double)completedSetsCount / uniqueStudentsCount * 100, 2)
                : 0.0;

            // Общее количество изученных карточек (карточки с IsMastered = true)
            var totalMasteredCards = await _context.UserFlashcardProgresses
                .Where(p => p.Flashcard.FlashcardSetId == id && p.IsMastered)
                .Select(p => p.FlashcardId)
                .Distinct()
                .CountAsync();

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
                CardsToReview = cardsToReviewForUser,
                AverageProgress = averageProgress, // Средний процент завершенных наборов
                TotalMasteredCards = totalMasteredCards, // Уникальных карточек, которые освоили хотя бы один пользователь
                CompletedSetsCount = completedSetsCount // Количество пользователей, которые полностью изучили набор
            });
        }

        /// <summary>
        /// Отметить повторение карточки (Spaced Repetition) - свои или из публичных наборов
        /// Создает/обновляет UserFlashcardProgress для индивидуального прогресса пользователя
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

            // Получаем или создаем прогресс пользователя по карточке
            var progress = await _context.UserFlashcardProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.FlashcardId == dto.FlashcardId);

            // Сохраняем старое состояние для инкрементального обновления статистики
            bool wasMasteredBefore = progress?.IsMastered ?? false;
            bool wasReviewedBefore = progress?.LastReviewedAt != null;
            bool isFirstReview = progress == null;

            if (progress == null)
            {
                // Создаем новую запись прогресса
                progress = new UserFlashcardProgress
                {
                    UserId = userId,
                    FlashcardId = dto.FlashcardId,
                    EaseFactor = 2.5, // Начальное значение
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserFlashcardProgresses.Add(progress);
            }

            // Применяем алгоритм интервального повторения к прогрессу пользователя
            _spacedRepetitionService.UpdateUserFlashcardProgress(progress, dto.Quality);

            // Определяем, изменился ли статус освоения
            bool isMasteredNow = progress.IsMastered;
            bool becameMastered = !wasMasteredBefore && isMasteredNow;
            bool lostMastered = wasMasteredBefore && !isMasteredNow;
            bool isReviewedNow = progress.LastReviewedAt != null;
            bool becameReviewed = !wasReviewedBefore && isReviewedNow;

            // Обновляем статистику набора
            var setAccess = await _context.UserFlashcardSetAccesses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.FlashcardSetId == card.FlashcardSetId);

            var now = DateTime.UtcNow;
            
            // ИСПРАВЛЕНИЕ: Создаем UserFlashcardSetAccess если его нет
            if (setAccess == null)
            {
                // Получаем количество карточек в наборе
                var totalCardsInSet = await _context.Flashcards
                    .Where(f => f.FlashcardSetId == card.FlashcardSetId)
                    .CountAsync();

                setAccess = new UserFlashcardSetAccess
                {
                    UserId = userId,
                    FlashcardSetId = card.FlashcardSetId,
                    FirstAccessedAt = now,
                    LastAccessedAt = now,
                    AccessCount = 1,
                    TotalCardsCount = totalCardsInSet,
                    CardsStudiedCount = isMasteredNow ? 1 : 0, // Если карточка уже освоена при первом доступе
                    IsCompleted = false,
                    CreatedAt = now
                };
                _context.UserFlashcardSetAccesses.Add(setAccess);
                
                _logger.LogInformation("Создан UserFlashcardSetAccess для userId={UserId}, setId={SetId} при review карточки", 
                    userId, card.FlashcardSetId);
            }
            else
            {
                // ОПТИМИЗАЦИЯ: Обновляем счетчики инкрементально вместо полного пересчета
                if (becameMastered)
                {
                    setAccess.CardsStudiedCount++;
                }
                else if (lostMastered)
                {
                    setAccess.CardsStudiedCount = Math.Max(0, setAccess.CardsStudiedCount - 1);
                }
                
                setAccess.LastAccessedAt = now;
                setAccess.UpdatedAt = now;
                setAccess.AccessCount++;
            }

            // Обновляем TotalCardsCount только если набор мог измениться (при создании setAccess уже обновлено)
            // Можно добавить проверку, но для простоты оставляем как есть

            // Проверяем, полностью ли изучен набор
            if (setAccess.CardsStudiedCount >= setAccess.TotalCardsCount && setAccess.TotalCardsCount > 0 && !setAccess.IsCompleted)
                {
                    setAccess.IsCompleted = true;
                setAccess.CompletedAt = now;
                
                _logger.LogInformation("Набор карточек завершен! userId={UserId}, setId={SetId}, masteredCards={Mastered}/{Total}", 
                    userId, card.FlashcardSetId, setAccess.CardsStudiedCount, setAccess.TotalCardsCount);
                }

            // ОПТИМИЗАЦИЯ: Обновляем общую статистику пользователя инкрементально
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && becameReviewed)
            {
                // Карточка просмотрена впервые - увеличиваем счетчик
                user.TotalCardsStudied++;
            }

            await _context.SaveChangesAsync();

            var result = new ReviewResultDto
            {
                FlashcardId = card.Id,
                NextReviewDate = progress.NextReviewDate ?? DateTime.UtcNow,
                IntervalDays = progress.Interval,
                Message = dto.Quality >= 3 
                    ? $"Отлично! Следующее повторение через {progress.Interval} дн." 
                    : "Попробуй ещё раз!"
            };

            return Ok(result);
        }
    }
}
