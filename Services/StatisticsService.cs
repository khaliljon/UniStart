using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniStart.Models;
using UniStart.Repositories;

namespace UniStart.Services;

/// <summary>
/// Service implementation for platform statistics and analytics
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatisticsService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Dictionary<string, object>> GetPlatformStatsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalQuizzes = await _unitOfWork.Quizzes.CountAsync();
        var totalExams = await _unitOfWork.Exams.CountAsync();
        var totalFlashcardSets = await _unitOfWork.FlashcardSets.CountAsync();
        var totalFlashcards = await _unitOfWork.Repository<Flashcard>().CountAsync();
        var totalQuizAttempts = await _unitOfWork.QuizAttempts.CountAsync();
        var totalExamAttempts = await _unitOfWork.ExamAttempts.CountAsync();

        var today = DateTime.UtcNow.Date;
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var monthAgo = DateTime.UtcNow.AddMonths(-1);

        var activeToday = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date >= today)
            .CountAsync();
        
        var activeThisWeek = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt >= weekAgo)
            .CountAsync();
        
        var activeThisMonth = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt >= monthAgo)
            .CountAsync();

        return new Dictionary<string, object>
        {
            ["totalUsers"] = totalUsers,
            ["totalQuizzes"] = totalQuizzes,
            ["totalExams"] = totalExams,
            ["totalFlashcardSets"] = totalFlashcardSets,
            ["totalFlashcards"] = totalFlashcards,
            ["totalQuizAttempts"] = totalQuizAttempts,
            ["totalExamAttempts"] = totalExamAttempts,
            ["activeToday"] = activeToday,
            ["activeThisWeek"] = activeThisWeek,
            ["activeThisMonth"] = activeThisMonth
        };
    }

    public async Task<Dictionary<string, object>> GetUserStatsAsync(string userId)
    {
        var quizAttempts = await _unitOfWork.QuizAttempts.GetUserAttemptsAsync(userId);
        var examAttempts = await _unitOfWork.ExamAttempts.GetUserAttemptsAsync(userId);
        
        var masteredCards = await _unitOfWork.FlashcardProgress.CountAsync(
            p => p.UserId == userId && p.IsMastered);
        
        var reviewedCards = await _unitOfWork.FlashcardProgress.CountAsync(
            p => p.UserId == userId && p.LastReviewedAt != null);

        return new Dictionary<string, object>
        {
            ["quizAttempts"] = quizAttempts.Count(),
            ["examAttempts"] = examAttempts.Count(),
            ["masteredCards"] = masteredCards,
            ["reviewedCards"] = reviewedCards,
            ["averageQuizScore"] = quizAttempts.Any() ? quizAttempts.Average(a => a.Percentage) : 0,
            ["averageExamScore"] = examAttempts.Any() ? examAttempts.Average(a => a.Percentage) : 0
        };
    }

    public async Task<Dictionary<string, object>> GetDailyActivityAsync(int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var dailyActivity = await _unitOfWork.QuizAttempts.Query()
            .Where(qa => qa.CompletedAt >= startDate)
            .GroupBy(qa => qa.CompletedAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Attempts = g.Count(),
                UniqueUsers = g.Select(qa => qa.UserId).Distinct().Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return new Dictionary<string, object>
        {
            ["period"] = $"Last {days} days",
            ["activity"] = dailyActivity
        };
    }

    public async Task<Dictionary<string, object>> GetHourlyActivityAsync()
    {
        var last24Hours = DateTime.UtcNow.AddHours(-24);

        var hourlyActivity = await _unitOfWork.QuizAttempts.Query()
            .Where(qa => qa.CompletedAt >= last24Hours)
            .GroupBy(qa => qa.CompletedAt!.Value.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync();

        return new Dictionary<string, object>
        {
            ["period"] = "Last 24 hours",
            ["activity"] = hourlyActivity
        };
    }

    public async Task<IEnumerable<object>> GetTopUsersAsync(int count)
    {
        var startDate = DateTime.UtcNow.AddDays(-30);

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
            .Take(count)
            .ToListAsync();

        var result = new List<object>();
        foreach (var userStat in topUsers)
        {
            var user = await _userManager.FindByIdAsync(userStat.UserId);
            if (user != null)
            {
                result.Add(new
                {
                    UserName = user.UserName ?? user.Email,
                    userStat.AttemptsCount,
                    AverageScore = Math.Round(userStat.AverageScore, 2)
                });
            }
        }

        return result;
    }

    public async Task<IEnumerable<object>> GetPopularSubjectsAsync(int count)
    {
        var popularSubjects = await _unitOfWork.Quizzes.Query()
            .GroupBy(q => q.Subject)
            .Select(g => new
            {
                Subject = g.Key,
                QuizCount = g.Count(),
                TotalAttempts = g.SelectMany(q => q.Attempts).Count()
            })
            .OrderByDescending(x => x.TotalAttempts)
            .Take(count)
            .ToListAsync();

        return popularSubjects;
    }

    public async Task<IEnumerable<object>> GetPopularQuizzesAsync(int count)
    {
        var popularQuizzes = await _unitOfWork.Quizzes.Query()
            .Include(q => q.Attempts)
            .Include(q => q.User)
            .Select(q => new
            {
                q.Id,
                q.Title,
                q.Subject,
                CreatedBy = q.User.UserName,
                AttemptsCount = q.Attempts.Count,
                AverageScore = q.Attempts.Any() ? q.Attempts.Average(a => a.Percentage) : 0
            })
            .OrderByDescending(x => x.AttemptsCount)
            .Take(count)
            .ToListAsync();

        return popularQuizzes;
    }
}
