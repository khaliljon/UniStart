using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AchievementsController> _logger;

    public AchievementsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AchievementsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Получить все доступные достижения
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllAchievements()
    {
        var userId = GetUserId();

        var achievements = await _context.Achievements.ToListAsync();
        var userAchievements = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var result = achievements.Select(a =>
        {
            var userAch = userAchievements.FirstOrDefault(ua => ua.AchievementId == a.Id);
            return new
            {
                a.Id,
                Name = a.Title,
                a.Description,
                a.Icon,
                Category = a.Type,
                PointsRequired = a.TargetValue,
                a.Level,
                IsUnlocked = userAch?.IsCompleted ?? false,
                Progress = userAch?.Progress ?? 0,
                UnlockedAt = userAch?.CompletedAt
            };
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Получить свои разблокированные достижения
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyAchievements()
    {
        var userId = GetUserId();

        var userAchievements = await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId && ua.IsCompleted)
            .OrderByDescending(ua => ua.CompletedAt)
            .Select(ua => new
            {
                ua.Id,
                Achievement = new
                {
                    ua.Achievement.Id,
                    Name = ua.Achievement.Title,
                    ua.Achievement.Description,
                    ua.Achievement.Icon,
                    Category = ua.Achievement.Type,
                    ua.Achievement.Level
                },
                UnlockedAt = ua.CompletedAt,
                ua.Progress
            })
            .ToListAsync();

        return Ok(userAchievements);
    }

    /// <summary>
    /// Получить прогресс по всем достижениям
    /// </summary>
    [HttpGet("progress")]
    public async Task<ActionResult<object>> GetAchievementsProgress()
    {
        var userId = GetUserId();

        var allAchievements = await _context.Achievements.CountAsync();
        var unlockedAchievements = await _context.UserAchievements
            .Where(ua => ua.UserId == userId && ua.IsCompleted)
            .CountAsync();

        var inProgress = await _context.UserAchievements
            .Where(ua => ua.UserId == userId && ua.Progress > 0 && !ua.IsCompleted)
            .Include(ua => ua.Achievement)
            .Select(ua => new
            {
                AchievementId = ua.Achievement.Id,
                AchievementName = ua.Achievement.Title,
                ua.Progress,
                TargetValue = ua.Achievement.TargetValue
            })
            .ToListAsync();

        return Ok(new
        {
            TotalAchievements = allAchievements,
            UnlockedAchievements = unlockedAchievements,
            Percentage = allAchievements > 0 ? Math.Round((unlockedAchievements * 100.0) / allAchievements, 2) : 0,
            InProgress = inProgress
        });
    }

    /// <summary>
    /// Проверить и обновить прогресс достижений пользователя
    /// </summary>
    [HttpPost("check-progress")]
    public async Task<ActionResult<object>> CheckAndUpdateProgress()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        var newlyUnlocked = new List<Achievement>();

        // Проверяем достижения по квизам
        var quizAttempts = await _context.UserQuizAttempts
            .Where(qa => qa.UserId == userId)
            .ToListAsync();

        // Достижение: "Первый шаг" - пройти 1 квиз
        await CheckAchievement(userId, "first", 1, quizAttempts.Count, newlyUnlocked);

        // Достижение: "Ученик" - пройти 10 квизов
        await CheckAchievement(userId, "learner", 10, quizAttempts.Count, newlyUnlocked);

        // Достижение: "Эксперт" - пройти 50 квизов
        await CheckAchievement(userId, "expert", 50, quizAttempts.Count, newlyUnlocked);

        // Достижение: "Отличник" - средний балл 90%+
        if (quizAttempts.Any())
        {
            var avgPercentage = (int)quizAttempts.Average(qa => qa.Percentage);
            await CheckAchievement(userId, "straight", 90, avgPercentage, newlyUnlocked);
        }

        // Проверяем достижения по карточкам
        var flashcardSets = await _context.FlashcardSets
            .Where(fs => fs.UserId == userId)
            .Include(fs => fs.Flashcards)
            .ToListAsync();

        var totalCards = flashcardSets.Sum(fs => fs.Flashcards.Count);

        // Достижение: "Создатель" - создать 5 наборов карточек
        await CheckAchievement(userId, "creator", 5, flashcardSets.Count, newlyUnlocked);

        // Достижение: "Коллекционер" - создать 100 карточек
        await CheckAchievement(userId, "collector", 100, totalCards, newlyUnlocked);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Прогресс обновлён",
            NewlyUnlocked = newlyUnlocked.Select(a => new { a.Id, Name = a.Title, a.Description })
        });
    }

    private async Task CheckAchievement(string userId, string achievementKeyword, int targetValue, int currentValue, List<Achievement> newlyUnlocked)
    {
        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.Title.ToLower().Contains(achievementKeyword) || a.Type.ToLower().Contains(achievementKeyword));

        if (achievement == null)
            return;

        var progress = Math.Min(currentValue, achievement.TargetValue);
        var isCompleted = currentValue >= achievement.TargetValue;

        var userAchievement = await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);

        if (userAchievement == null)
        {
            userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                Progress = progress,
                IsCompleted = isCompleted,
                CompletedAt = isCompleted ? DateTime.UtcNow : null
            };
            _context.UserAchievements.Add(userAchievement);

            if (isCompleted)
            {
                newlyUnlocked.Add(achievement);
                _logger.LogInformation("Пользователь {UserId} разблокировал достижение {Achievement}", userId, achievement.Title);
            }
        }
        else if (!userAchievement.IsCompleted)
        {
            userAchievement.Progress = progress;
            
            if (isCompleted)
            {
                userAchievement.IsCompleted = true;
                userAchievement.CompletedAt = DateTime.UtcNow;
                newlyUnlocked.Add(achievement);
                _logger.LogInformation("Пользователь {UserId} разблокировал достижение {Achievement}", userId, achievement.Title);
            }
        }
    }

    /// <summary>
    /// Создать новое достижение (только для админов)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<Achievement>> CreateAchievement([FromBody] CreateAchievementDto dto)
    {
        var achievement = new Achievement
        {
            Title = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Type = dto.Category,
            TargetValue = dto.PointsRequired,
            Level = dto.Level
        };

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создано достижение: {Name}", achievement.Title);

        return CreatedAtAction(nameof(GetAllAchievements), new { id = achievement.Id }, achievement);
    }
}

// DTOs для Achievements
public class CreateAchievementDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏆";
    public string Category { get; set; } = "General";
    public int PointsRequired { get; set; } = 0;
    public int Level { get; set; } = 1;
}
