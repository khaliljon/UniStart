using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Services;

namespace UniStart.Controllers.Flashcards
{
    [ApiController]
    [Route("api/flashcards")]
    [Authorize]
    public class FlashcardsStudyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpacedRepetitionService _spacedRepetitionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<FlashcardsStudyController> _logger;

        public FlashcardsStudyController(
            ApplicationDbContext context,
            ISpacedRepetitionService spacedRepetitionService,
            UserManager<ApplicationUser> userManager,
            ILogger<FlashcardsStudyController> logger)
        {
            _context = context;
            _spacedRepetitionService = spacedRepetitionService;
            _userManager = userManager;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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
