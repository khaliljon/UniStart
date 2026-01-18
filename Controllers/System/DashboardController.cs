using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniStart.Data;
using UniStart.Services;

namespace UniStart.Controllers.System
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            IStatisticsService statisticsService,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Получить общую статистику пользователя
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult> GetUserStatistics()
        {
            var userId = GetUserId();

            // Статистика по флешкартам
            var flashcardSetsCount = await _context.FlashcardSets
                .Where(fs => fs.UserId == userId)
                .CountAsync();

            var totalFlashcards = await _context.FlashcardSets
                .Where(fs => fs.UserId == userId)
                .SelectMany(fs => fs.Flashcards)
                .CountAsync();

            // Используем UserFlashcardProgress для подсчета карточек к повторению
            var cardsToReview = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && (p.NextReviewDate == null || p.NextReviewDate <= DateTime.UtcNow))
                .CountAsync();

            // Используем UserFlashcardProgress для подсчета изученных карточек
            var studiedCards = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && p.LastReviewedAt != null)
                .CountAsync();

            // Статистика по квизам
            var createdQuizzesCount = await _context.Quizzes
                .Where(q => q.UserId == userId)
                .CountAsync();

            var quizAttempts = await _context.UserQuizAttempts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var totalQuizzesTaken = quizAttempts
                .Select(a => a.QuizId)
                .Distinct()
                .Count();

            var totalAttempts = quizAttempts.Count;

            var averageScore = quizAttempts.Any() 
                ? quizAttempts.Average(a => a.Percentage) 
                : 0;

            var bestScore = quizAttempts.Any() 
                ? quizAttempts.Max(a => a.Percentage) 
                : 0;

            var stats = new
            {
                Flashcards = new
                {
                    TotalSets = flashcardSetsCount,
                    TotalCards = totalFlashcards,
                    StudiedCards = studiedCards,
                    CardsToReview = cardsToReview,
                    ProgressPercentage = totalFlashcards > 0 
                        ? Math.Round((double)studiedCards / totalFlashcards * 100, 2) 
                        : 0
                },
                Quizzes = new
                {
                    CreatedQuizzes = createdQuizzesCount,
                    TotalQuizzesTaken = totalQuizzesTaken,
                    TotalAttempts = totalAttempts,
                    AverageScore = Math.Round(averageScore, 2),
                    BestScore = Math.Round(bestScore, 2)
                },
                Activity = new
                {
                    TotalStudyTime = quizAttempts.Sum(a => a.TimeSpentSeconds),
                    LastActivity = await GetLastActivityDate(userId)
                }
            };

            return Ok(stats);
        }

        /// <summary>
        /// Получить прогресс по флешкартам за последние 7 дней
        /// </summary>
        [HttpGet("flashcards/progress")]
        public async Task<ActionResult> GetFlashcardProgress([FromQuery] int days = 7)
        {
            var userId = GetUserId();
            var startDate = DateTime.UtcNow.AddDays(-days);

            // Используем UserFlashcardProgress для подсчета прогресса
            var reviewedCards = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && p.LastReviewedAt != null && p.LastReviewedAt >= startDate)
                .GroupBy(p => p.LastReviewedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    CardsReviewed = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(reviewedCards);
        }

        /// <summary>
        /// Получить результаты последних 10 квизов
        /// </summary>
        [HttpGet("quizzes/recent")]
        public async Task<ActionResult> GetRecentQuizResults([FromQuery] int limit = 10)
        {
            var userId = GetUserId();

            var recentAttempts = await _context.UserQuizAttempts
                .Where(a => a.UserId == userId)
                .Include(a => a.Quiz)
                .OrderByDescending(a => a.CompletedAt)
                .Take(limit)
                .Select(a => new
                {
                    a.Id,
                    QuizTitle = a.Quiz.Title,
                    a.Score,
                    a.MaxScore,
                    a.Percentage,
                    a.TimeSpentSeconds,
                    a.CompletedAt
                })
                .ToListAsync();

            return Ok(recentAttempts);
        }

        /// <summary>
        /// Получить breakdown по предметам
        /// </summary>
        [HttpGet("subjects/breakdown")]
        public async Task<ActionResult> GetSubjectBreakdown()
        {
            var userId = GetUserId();

            var subjectStats = await _context.UserQuizAttempts
                .Where(a => a.UserId == userId)
                .Include(a => a.Quiz)
                .GroupBy(a => a.Quiz.Subject)
                .Select(g => new
                {
                    Subject = g.Key,
                    TotalAttempts = g.Count(),
                    AverageScore = Math.Round(g.Average(a => a.Percentage), 2),
                    BestScore = Math.Round(g.Max(a => a.Percentage), 2)
                })
                .ToListAsync();

            return Ok(subjectStats);
        }

        /// <summary>
        /// Получить предстоящие повторения карточек
        /// </summary>
        [HttpGet("flashcards/upcoming")]
        public async Task<ActionResult> GetUpcomingReviews()
        {
            var userId = GetUserId();
            var today = DateTime.UtcNow.Date;

            var upcomingReviews = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && p.NextReviewDate != null && p.NextReviewDate >= today)
                .GroupBy(p => p.NextReviewDate!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    CardsCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .Take(14) // Следующие 2 недели
                .ToListAsync();

            return Ok(upcomingReviews);
        }

        private async Task<DateTime?> GetLastActivityDate(string userId)
        {
            var lastFlashcardReview = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && p.LastReviewedAt != null)
                .MaxAsync(p => (DateTime?)p.LastReviewedAt);

            var lastQuizAttempt = await _context.UserQuizAttempts
                .Where(a => a.UserId == userId)
                .MaxAsync(a => (DateTime?)a.CompletedAt);

            if (lastFlashcardReview == null && lastQuizAttempt == null)
                return null;

            if (lastFlashcardReview == null)
                return lastQuizAttempt;

            if (lastQuizAttempt == null)
                return lastFlashcardReview;

            return lastFlashcardReview > lastQuizAttempt 
                ? lastFlashcardReview 
                : lastQuizAttempt;
        }
    }
}
