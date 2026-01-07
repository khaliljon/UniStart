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

namespace UniStart.Controllers.Social;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreaksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StreaksController> _logger;

    public StreaksController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<StreaksController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Получить свою статистику стримов
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<object>> GetMyStreak()
    {
        var userId = GetUserId();

        var streak = await _context.UserStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (streak == null)
        {
            // Создаём новый streak
            streak = new UserStreak
            {
                UserId = userId,
                CurrentStreak = 0,
                LongestStreak = 0,
                LastActivityDate = DateTime.MinValue,
                TotalActiveDays = 0
            };
            _context.UserStreaks.Add(streak);
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            streak.CurrentStreak,
            streak.LongestStreak,
            streak.LastActivityDate,
            streak.TotalActiveDays,
            IsActiveToday = streak.LastActivityDate.Date == DateTime.UtcNow.Date
        });
    }

    /// <summary>
    /// Обновить streak (вызывается при любой активности пользователя)
    /// </summary>
    [HttpPost("update")]
    public async Task<ActionResult<object>> UpdateStreak()
    {
        var userId = GetUserId();

        var streak = await _context.UserStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (streak == null)
        {
            streak = new UserStreak { UserId = userId };
            _context.UserStreaks.Add(streak);
        }

        var today = DateTime.UtcNow.Date;
        var lastActivity = streak.LastActivityDate.Date;

        // Если сегодня уже была активность, ничего не делаем
        if (lastActivity == today)
        {
            return Ok(new { Message = "Активность уже учтена сегодня", streak.CurrentStreak });
        }

        // Проверяем, не пропустил ли пользователь дни
        var daysDifference = (today - lastActivity).Days;

        if (daysDifference == 1)
        {
            // Продолжаем streak
            streak.CurrentStreak++;
        }
        else if (daysDifference > 1)
        {
            // Streak прервался
            streak.CurrentStreak = 1;
        }
        else
        {
            // Первая активность
            streak.CurrentStreak = 1;
        }

        // Обновляем лучший streak
        if (streak.CurrentStreak > streak.LongestStreak)
        {
            streak.LongestStreak = streak.CurrentStreak;
        }

        streak.LastActivityDate = DateTime.UtcNow;
        streak.TotalActiveDays++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлён streak пользователя {UserId}: {CurrentStreak} дней", userId, streak.CurrentStreak);

        return Ok(new
        {
            Message = "Streak обновлён!",
            streak.CurrentStreak,
            streak.LongestStreak,
            IsNewRecord = streak.CurrentStreak == streak.LongestStreak && streak.CurrentStreak > 1
        });
    }

    /// <summary>
    /// Получить топ пользователей по стримам
    /// </summary>
    [HttpGet("leaderboard")]
    public async Task<ActionResult<object>> GetStreakLeaderboard([FromQuery] int top = 10)
    {
        var leaderboard = await _context.UserStreaks
            .Include(s => s.User)
            .OrderByDescending(s => s.CurrentStreak)
            .Take(top)
            .Select(s => new
            {
                UserId = s.User.Id,
                UserName = s.User.UserName ?? s.User.Email,
                s.CurrentStreak,
                s.LongestStreak,
                s.TotalActiveDays
            })
            .ToListAsync();

        return Ok(leaderboard);
    }
}
