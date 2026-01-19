using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Controllers.Student
{
    [ApiController]
    [Route("api/student")]
    [Authorize]
    public class StudentProgressController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentProgressController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User) 
            ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

        /// <summary>
        /// Получить полный прогресс студента (для страницы прогресса)
        /// </summary>
        [HttpGet("progress")]
        public async Task<ActionResult<object>> GetProgress()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { Message = "Пользователь не найден" });

            // Статистика по квизам
            var quizAttempts = await _context.UserQuizAttempts
                .Where(qa => qa.UserId == userId)
                .Include(qa => qa.Quiz)
                .ToListAsync();

            var totalQuizzesTaken = quizAttempts.Select(qa => qa.QuizId).Distinct().Count();
            var averageQuizScore = quizAttempts.Any() ? quizAttempts.Average(qa => qa.Percentage) : 0;
            var totalTimeSpent = quizAttempts.Sum(qa => qa.TimeSpentSeconds);

            // Серия (streak) - считаем последовательные дни с активностью
            var activeDates = quizAttempts
                .Select(qa => qa.CompletedAt!.Value.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int currentStreak = 0;
            int longestStreak = 0;
            int tempStreak = 0;

            for (int i = 0; i < activeDates.Count; i++)
            {
                if (i == 0 || (activeDates[i - 1] - activeDates[i]).Days == 1)
                {
                    tempStreak++;
                    if (i == 0) currentStreak = tempStreak;
                }
                else
                {
                    if (i > 0) currentStreak = 0;
                    tempStreak = 1;
                }
                longestStreak = Math.Max(longestStreak, tempStreak);
            }

            // Достижения (пока 0, можно расширить)
            var totalAchievements = 0;

            // Прогресс по предметам (будет использовать Subjects коллекцию)
            // TODO: Реализовать после обновления фронтенда на новую систему Subjects
            var subjectProgress = new List<object>();

            // Недавняя активность
            var recentActivity = quizAttempts
                .OrderByDescending(qa => qa.CompletedAt)
                .Take(10)
                .Select(qa => new
                {
                    id = qa.Id,
                    type = "quiz",
                    title = qa.Quiz.Title,
                    score = (int)qa.Percentage,
                    date = qa.CompletedAt
                })
                .ToList();

            // Статистика по карточкам (ДОПОЛНЕНО)
            var reviewedCards = user.TotalCardsStudied; // Обновляется в FlashcardsController.ReviewFlashcard
            var masteredCards = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && p.IsMastered)
                .CountAsync();
            var completedFlashcardSets = await _context.UserFlashcardSetAccesses
                .Where(a => a.UserId == userId && a.IsCompleted)
                .CountAsync();

            return Ok(new
            {
                stats = new
                {
                    totalCardsStudied = reviewedCards, // Просмотрено карточек
                    masteredCards, // Освоено карточек
                    completedFlashcardSets, // Завершено наборов
                    totalQuizzesTaken,
                    averageQuizScore = Math.Round(averageQuizScore, 1),
                    totalTimeSpent,
                    currentStreak,
                    longestStreak,
                    totalAchievements
                },
                recentActivity,
                subjectProgress
            });
        }

        /// <summary>
        /// Получить свою статистику обучения
        /// </summary>
        [HttpGet("my-stats")]
        public async Task<ActionResult<object>> GetMyStats()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { Message = "Пользователь не найден" });

            // Статистика по карточкам
            var myFlashcardSets = await _context.FlashcardSets
                .Where(fs => fs.UserId == userId)
                .Include(fs => fs.Flashcards)
                .ToListAsync();

            var totalFlashcardSets = myFlashcardSets.Count;
            var totalFlashcards = myFlashcardSets.Sum(fs => fs.Flashcards.Count);

            // Статистика по квизам
            var myQuizAttempts = await _context.UserQuizAttempts
                .Where(qa => qa.UserId == userId)
                .Include(qa => qa.Quiz)
                .ToListAsync();

            var totalQuizzesTaken = myQuizAttempts.Select(qa => qa.QuizId).Distinct().Count();
            var totalAttempts = myQuizAttempts.Count;
            var averageScore = myQuizAttempts.Any() ? Math.Round(myQuizAttempts.Average(qa => qa.Percentage), 2) : 0;
            var bestScore = myQuizAttempts.Any() ? myQuizAttempts.Max(qa => qa.Percentage) : 0;

            // Последние попытки
            var recentAttempts = myQuizAttempts
                .OrderByDescending(qa => qa.CompletedAt)
                .Take(5)
                .Select(qa => new
                {
                    qa.Id,
                    QuizTitle = qa.Quiz.Title,
                    qa.Score,
                    qa.MaxScore,
                    qa.Percentage,
                    qa.CompletedAt
                })
                .ToList();

            // Активность за последние 7 дней
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var attemptsLastWeek = myQuizAttempts.Count(qa => qa.CompletedAt >= sevenDaysAgo);
            var cardsStudiedLastWeek = user.TotalCardsStudied; // Можно улучшить, если отслеживать по датам

            return Ok(new
            {
                User = new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.CreatedAt
                },
                FlashcardStats = new
                {
                    TotalSets = totalFlashcardSets,
                    TotalCards = totalFlashcards,
                    TotalStudied = user.TotalCardsStudied
                },
                QuizStats = new
                {
                    TotalQuizzesTaken = totalQuizzesTaken,
                    TotalAttempts = totalAttempts,
                    AverageScore = averageScore,
                    BestScore = bestScore
                },
                RecentActivity = new
                {
                    RecentAttempts = recentAttempts,
                    AttemptsLastWeek = attemptsLastWeek,
                    CardsStudiedLastWeek = cardsStudiedLastWeek
                }
            });
        }

        /// <summary>
        /// Получить прогресс изучения по предметам
        /// </summary>
        [HttpGet("progress-by-subject")]
        public async Task<ActionResult<object>> GetProgressBySubject()
        {
            var userId = GetUserId();

            // Subject field removed from Quiz model
            var progressBySubject = new List<object>();

            return Ok(progressBySubject);
        }

        /// <summary>
        /// Получить историю изучения (график активности)
        /// </summary>
        [HttpGet("study-history")]
        public async Task<ActionResult<object>> GetStudyHistory([FromQuery] int days = 30)
        {
            var userId = GetUserId();
            var startDate = DateTime.UtcNow.AddDays(-days);

            var quizAttempts = await _context.UserQuizAttempts
                .Where(qa => qa.UserId == userId && qa.CompletedAt >= startDate)
                .ToListAsync();

            var dailyStats = quizAttempts
                .GroupBy(qa => qa.CompletedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AttemptsCount = g.Count(),
                    AverageScore = Math.Round(g.Average(qa => qa.Percentage), 2),
                    TotalTimeSpent = g.Sum(qa => qa.TimeSpentSeconds)
                })
                .OrderBy(s => s.Date)
                .ToList();

            return Ok(new
            {
                Period = $"Last {days} days",
                TotalDaysActive = dailyStats.Count,
                DailyStats = dailyStats
            });
        }
    }
}
