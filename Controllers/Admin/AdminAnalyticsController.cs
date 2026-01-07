using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
using UniStart.Repositories;

namespace UniStart.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminAnalyticsController> _logger;

    public AdminAnalyticsController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminAnalyticsController> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Получить статистику платформы
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetPlatformStats()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalFlashcardSets = await _unitOfWork.FlashcardSets.CountAsync();
        var totalFlashcards = await _unitOfWork.Repository<Flashcard>().CountAsync();
        var totalQuizzes = await _unitOfWork.Quizzes.CountAsync();
        var totalQuizAttempts = await _unitOfWork.QuizAttempts.CountAsync();

        // Пользователи по ролям
        var adminCount = 0;
        var teacherCount = 0;
        var studentCount = 0;

        var allUsers = await _userManager.Users.ToListAsync();
        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(UserRoles.Admin)) adminCount++;
            if (roles.Contains(UserRoles.Teacher)) teacherCount++;
            if (roles.Contains(UserRoles.Student)) studentCount++;
        }

        // Активность за последние 7 дней
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var newUsersLastWeek = await _userManager.Users
            .Where(u => u.CreatedAt >= sevenDaysAgo)
            .CountAsync();

        var newQuizzesLastWeek = await _unitOfWork.Quizzes.CountAsync(q => q.CreatedAt >= sevenDaysAgo);
        var quizAttemptsLastWeek = await _unitOfWork.QuizAttempts.CountAsync(qa => qa.CompletedAt >= sevenDaysAgo);

        return Ok(new
        {
            TotalUsers = totalUsers,
            TotalFlashcardSets = totalFlashcardSets,
            TotalFlashcards = totalFlashcards,
            TotalQuizzes = totalQuizzes,
            TotalQuizAttempts = totalQuizAttempts,
            UsersByRole = new
            {
                Admins = adminCount,
                Teachers = teacherCount,
                Students = studentCount
            },
            ActivityLastWeek = new
            {
                NewUsers = newUsersLastWeek,
                NewQuizzes = newQuizzesLastWeek,
                QuizAttempts = quizAttemptsLastWeek
            }
        });
    }

    /// <summary>
    /// Получить основную аналитику платформы
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<object>> GetAnalytics()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalQuizzes = await _unitOfWork.Quizzes.CountAsync();
        var totalExams = await _unitOfWork.Exams.CountAsync();
        var totalFlashcardSets = await _unitOfWork.FlashcardSets.CountAsync();
        var totalQuestions = await _unitOfWork.Repository<QuizQuestion>().CountAsync() + 
                             await _unitOfWork.Repository<ExamQuestion>().CountAsync();
        var totalFlashcards = await _unitOfWork.Repository<Flashcard>().CountAsync();
        var totalAttempts = await _unitOfWork.QuizAttempts.CountAsync();
        
        var today = DateTime.UtcNow.Date;
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var monthAgo = DateTime.UtcNow.AddMonths(-1);
        
        var activeToday = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date >= today)
            .CountAsync();
            
        var activeThisWeek = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date >= weekAgo)
            .CountAsync();
            
        var activeThisMonth = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date >= monthAgo)
            .CountAsync();
        
        var completedQuizAttempts = await _unitOfWork.QuizAttempts.FindAsync(qa => qa.CompletedAt != null);
        
        var averageQuizScore = completedQuizAttempts.Any() 
            ? completedQuizAttempts.Average(qa => qa.Percentage)
            : 0.0;
            
        var totalAchievements = await _unitOfWork.Achievements.CountAsync();
        
        var result = new
        {
            Stats = new
            {
                TotalUsers = totalUsers,
                TotalQuizzes = totalQuizzes,
                TotalExams = totalExams,
                TotalFlashcardSets = totalFlashcardSets,
                TotalQuestions = totalQuestions,
                TotalFlashcards = totalFlashcards,
                TotalAttempts = totalAttempts,
                ActiveToday = activeToday,
                ActiveThisWeek = activeThisWeek,
                ActiveThisMonth = activeThisMonth,
                AverageQuizScore = Math.Round(averageQuizScore, 2),
                TotalAchievements = totalAchievements
            }
        };
        
        _logger.LogInformation("Analytics data: Users={TotalUsers}, Quizzes={TotalQuizzes}, Exams={TotalExams}", 
            totalUsers, totalQuizzes, totalExams);
        
        return Ok(result);
    }

    /// <summary>
    /// Получить детальную аналитику платформы с графиками
    /// </summary>
    [HttpGet("analytics/detailed")]
    public async Task<ActionResult<object>> GetDetailedAnalytics([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // Активность по дням
        var dailyActivity = await _unitOfWork.QuizAttempts.Query()
            .Where(qa => qa.CompletedAt >= startDate)
            .GroupBy(qa => qa.CompletedAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                QuizAttempts = g.Count(),
                UniqueUsers = g.Select(qa => qa.UserId).Distinct().Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Популярные предметы
        var popularSubjects = await _unitOfWork.Quizzes.Query()
            .GroupBy(q => q.Subject)
            .Select(g => new
            {
                Subject = g.Key,
                QuizCount = g.Count(),
                TotalAttempts = g.SelectMany(q => q.Attempts).Count()
            })
            .OrderByDescending(x => x.TotalAttempts)
            .Take(10)
            .ToListAsync();

        // Топ активных пользователей
        var topUsers = await _unitOfWork.QuizAttempts.Query()
            .Where(qa => qa.CompletedAt >= startDate)
            .GroupBy(qa => qa.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                AttemptsCount = g.Count(),
                AverageScore = g.Average(qa => qa.Percentage)
            })
            .OrderByDescending(x => x.AttemptsCount)
            .Take(10)
            .ToListAsync();

        var topUsersWithDetails = new List<object>();
        foreach (var userStat in topUsers)
        {
            var user = await _userManager.FindByIdAsync(userStat.UserId);
            if (user != null)
            {
                topUsersWithDetails.Add(new
                {
                    UserName = user.UserName ?? user.Email,
                    userStat.AttemptsCount,
                    AverageScore = Math.Round(userStat.AverageScore, 2)
                });
            }
        }

        // Статистика по достижениям
        var achievementStats = await _unitOfWork.Repository<UserAchievement>().Query()
            .Where(ua => ua.IsCompleted)
            .GroupBy(ua => ua.AchievementId)
            .Select(g => new
            {
                AchievementId = g.Key,
                UnlockedBy = g.Count()
            })
            .OrderByDescending(x => x.UnlockedBy)
            .Take(10)
            .ToListAsync();

        return Ok(new
        {
            Period = $"Last {days} days",
            DailyActivity = dailyActivity,
            PopularSubjects = popularSubjects,
            TopUsers = topUsersWithDetails,
            AchievementStats = achievementStats
        });
    }

    /// <summary>
    /// Получить статистику по времени (почасовая активность)
    /// </summary>
    [HttpGet("analytics/hourly")]
    public async Task<ActionResult<object>> GetHourlyAnalytics()
    {
        var last24Hours = DateTime.UtcNow.AddHours(-24);

        var hourlyActivity = await _unitOfWork.QuizAttempts.Query()
            .Where(qa => qa.CompletedAt >= last24Hours)
            .GroupBy(qa => qa.CompletedAt!.Value.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Hour)
            .ToListAsync();

        return Ok(hourlyActivity);
    }
}
