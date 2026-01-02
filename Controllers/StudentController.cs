using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Все аутентифицированные пользователи
public class StudentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<StudentController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
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

        // Прогресс по предметам (квизы)
        var quizSubjectProgress = quizAttempts
            .GroupBy(qa => qa.Quiz.Subject)
            .Select(g => new
            {
                Subject = g.Key,
                QuizzesTaken = g.Select(qa => qa.QuizId).Distinct().Count(),
                AverageScore = Math.Round(g.Average(qa => qa.Percentage), 1)
            })
            .ToDictionary(x => x.Subject);

        // Прогресс по предметам (карточки)
        var flashcardsBySubject = await _context.UserFlashcardProgresses
            .Where(p => p.UserId == userId && p.LastReviewedAt != null)
            .Include(p => p.Flashcard)
                .ThenInclude(f => f.FlashcardSet)
            .GroupBy(p => p.Flashcard.FlashcardSet.Subject)
            .Select(g => new
            {
                Subject = g.Key,
                CardsStudied = g.Count(),
                MasteredCards = g.Count(p => p.IsMastered)
            })
            .ToListAsync();

        var flashcardSubjectDict = flashcardsBySubject.ToDictionary(x => x.Subject);

        // Объединяем статистику по предметам
        var allSubjects = quizSubjectProgress.Keys
            .Union(flashcardSubjectDict.Keys)
            .Distinct()
            .ToList();

        var subjectProgress = allSubjects.Select(subject =>
        {
            var hasQuizStats = quizSubjectProgress.TryGetValue(subject, out var quizStats);
            var hasFlashcardStats = flashcardSubjectDict.TryGetValue(subject, out var flashcardStats);

            return new
            {
                subject = subject,
                quizzesTaken = hasQuizStats ? quizStats.QuizzesTaken : 0,
                averageScore = hasQuizStats ? quizStats.AverageScore : 0.0,
                cardsStudied = hasFlashcardStats ? flashcardStats.CardsStudied : 0,
                masteredCards = hasFlashcardStats ? flashcardStats.MasteredCards : 0
            };
        }).ToList();

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
    /// Получить все свои наборы карточек
    /// </summary>
    [HttpGet("my-flashcard-sets")]
    public async Task<ActionResult<IEnumerable<FlashcardSetDto>>> GetMyFlashcardSets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var flashcardSets = await _context.FlashcardSets
            .Where(fs => fs.UserId == userId)
            .Include(fs => fs.Flashcards)
            .OrderByDescending(fs => fs.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(fs => new FlashcardSetDto
            {
                Id = fs.Id,
                Title = fs.Title,
                Description = fs.Description,
                Subject = fs.Subject,
                CardCount = fs.Flashcards.Count,
                CreatedAt = fs.CreatedAt,
                UpdatedAt = fs.UpdatedAt
            })
            .ToListAsync();

        return Ok(flashcardSets);
    }

    /// <summary>
    /// Получить все попытки прохождения квизов
    /// </summary>
    [HttpGet("my-quiz-attempts")]
    public async Task<ActionResult<object>> GetMyQuizAttempts(
        [FromQuery] int? quizId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var query = _context.UserQuizAttempts
            .Include(qa => qa.Quiz)
            .Where(qa => qa.UserId == userId);

        if (quizId.HasValue)
            query = query.Where(qa => qa.QuizId == quizId.Value);

        var attempts = await query
            .OrderByDescending(qa => qa.CompletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(qa => new
            {
                qa.Id,
                QuizId = qa.Quiz.Id,
                QuizTitle = qa.Quiz.Title,
                qa.Score,
                qa.MaxScore,
                qa.Percentage,
                qa.TimeSpentSeconds,
                qa.CompletedAt
            })
            .ToListAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            Attempts = attempts
        });
    }

    /// <summary>
    /// Получить доступные публичные квизы
    /// </summary>
    [HttpGet("available-quizzes")]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetAvailableQuizzes(
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Quizzes
            .Include(q => q.Questions)
            .Where(q => q.IsPublic); // Только публичные тесты

        if (!string.IsNullOrWhiteSpace(subject))
            query = query.Where(q => q.Subject.Contains(subject));

        if (!string.IsNullOrWhiteSpace(difficulty))
            query = query.Where(q => q.Difficulty == difficulty);

        var quizzes = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                Subject = q.Subject,
                Difficulty = q.Difficulty,
                TimeLimit = q.TimeLimit,
                QuestionCount = q.Questions.Count,
                IsPublic = q.IsPublic,
                CreatedAt = q.CreatedAt
            })
            .ToListAsync();

        return Ok(quizzes);
    }

    /// <summary>
    /// Получить доступные публичные наборы карточек
    /// </summary>
    [HttpGet("available-flashcard-sets")]
    public async Task<ActionResult<IEnumerable<FlashcardSetDto>>> GetAvailableFlashcardSets(
        [FromQuery] string? subject = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.FlashcardSets
            .Include(fs => fs.Flashcards)
            .Where(fs => fs.IsPublic); // Только публичные наборы

        if (!string.IsNullOrWhiteSpace(subject))
            query = query.Where(fs => fs.Subject.Contains(subject));

        var flashcardSets = await query
            .OrderByDescending(fs => fs.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(fs => new FlashcardSetDto
            {
                Id = fs.Id,
                Title = fs.Title,
                Description = fs.Description,
                Subject = fs.Subject,
                CardCount = fs.Flashcards.Count,
                CreatedAt = fs.CreatedAt,
                UpdatedAt = fs.UpdatedAt
            })
            .ToListAsync();

        return Ok(flashcardSets);
    }

    /// <summary>
    /// Получить прогресс изучения по предметам
    /// </summary>
    [HttpGet("progress-by-subject")]
    public async Task<ActionResult<object>> GetProgressBySubject()
    {
        var userId = GetUserId();

        var quizAttempts = await _context.UserQuizAttempts
            .Where(qa => qa.UserId == userId)
            .Include(qa => qa.Quiz)
            .ToListAsync();

        var progressBySubject = quizAttempts
            .GroupBy(qa => qa.Quiz.Subject)
            .Select(g => new
            {
                Subject = g.Key,
                TotalAttempts = g.Count(),
                AverageScore = Math.Round(g.Average(qa => qa.Percentage), 2),
                BestScore = g.Max(qa => qa.Percentage),
                LastAttempt = g.Max(qa => qa.CompletedAt)
            })
            .OrderByDescending(s => s.TotalAttempts)
            .ToList();

        return Ok(progressBySubject);
    }

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
