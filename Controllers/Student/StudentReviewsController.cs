using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers.Student
{
    [ApiController]
    [Route("api/student")]
    [Authorize]
    public class StudentReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentReviewsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User) 
            ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

        /// <summary>
        /// Получить рекомендации для повторения (на основе интервального повторения)
        /// </summary>
        [HttpGet("review-recommendations")]
        public async Task<ActionResult<object>> GetReviewRecommendations()
        {
            var userId = GetUserId();

            // Находим карточки, которые нужно повторить
            var flashcardSets = await _context.FlashcardSets
                .Where(fs => fs.UserId == userId)
                .Include(fs => fs.Flashcards)
                .ToListAsync();

            var recommendations = new List<object>();

            foreach (var set in flashcardSets)
            {
                var cardsNeedingReview = set.Flashcards
                    .Where(c => c.NextReviewDate <= DateTime.UtcNow)
                    .Count();

                if (cardsNeedingReview > 0)
                {
                    recommendations.Add(new
                    {
                        SetId = set.Id,
                        SetTitle = set.Title,
                        CardsToReview = cardsNeedingReview,
                        TotalCards = set.Flashcards.Count
                    });
                }
            }

            return Ok(new
            {
                TotalSetsNeedingReview = recommendations.Count,
                Recommendations = recommendations.OrderByDescending(r => ((dynamic)r).CardsToReview).ToList()
            });
        }

        /// <summary>
        /// Получить сравнение с другими студентами (Leaderboard)
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<ActionResult<object>> GetLeaderboard(
            [FromQuery] string period = "all-time", // all-time, week, month
            [FromQuery] int top = 10)
        {
            DateTime? startDate = period switch
            {
                "week" => DateTime.UtcNow.AddDays(-7),
                "month" => DateTime.UtcNow.AddMonths(-1),
                _ => null
            };

            var query = _context.UserQuizAttempts.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(qa => qa.CompletedAt >= startDate.Value);

            var userStats = await query
                .GroupBy(qa => qa.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalAttempts = g.Count(),
                    AverageScore = Math.Round(g.Average(qa => qa.Percentage), 2),
                    TotalPoints = g.Sum(qa => qa.Score)
                })
                .OrderByDescending(s => s.TotalPoints)
                .Take(top)
                .ToListAsync();

            // Получаем информацию о пользователях
            var leaderboard = new List<object>();
            int rank = 1;

            foreach (var stat in userStats)
            {
                var user = await _userManager.FindByIdAsync(stat.UserId);
                if (user != null)
                {
                    leaderboard.Add(new
                    {
                        Rank = rank++,
                        UserName = user.UserName ?? user.Email,
                        stat.TotalAttempts,
                        stat.AverageScore,
                        stat.TotalPoints
                    });
                }
            }

            return Ok(new
            {
                Period = period,
                Top = top,
                Leaderboard = leaderboard
            });
        }
    }
}
