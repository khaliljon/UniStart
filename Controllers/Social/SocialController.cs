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
public class SocialController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SocialController> _logger;

    public SocialController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<SocialController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Подписаться на пользователя
    /// </summary>
    [HttpPost("follow/{userId}")]
    public async Task<ActionResult> FollowUser(string userId)
    {
        var followerId = GetUserId();

        if (followerId == userId)
            return BadRequest(new { Message = "Нельзя подписаться на самого себя" });

        var userToFollow = await _userManager.FindByIdAsync(userId);
        if (userToFollow == null)
            return NotFound(new { Message = "Пользователь не найден" });

        var existingFollow = await _context.UserFollows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == userId);

        if (existingFollow != null)
            return BadRequest(new { Message = "Вы уже подписаны на этого пользователя" });

        var follow = new UserFollow
        {
            FollowerId = followerId,
            FollowingId = userId
        };

        _context.UserFollows.Add(follow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пользователь {FollowerId} подписался на {FollowingId}", followerId, userId);

        return Ok(new { Message = "Вы успешно подписались на пользователя" });
    }

    /// <summary>
    /// Отписаться от пользователя
    /// </summary>
    [HttpDelete("unfollow/{userId}")]
    public async Task<ActionResult> UnfollowUser(string userId)
    {
        var followerId = GetUserId();

        var follow = await _context.UserFollows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == userId);

        if (follow == null)
            return NotFound(new { Message = "Вы не подписаны на этого пользователя" });

        _context.UserFollows.Remove(follow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пользователь {FollowerId} отписался от {FollowingId}", followerId, userId);

        return Ok(new { Message = "Вы отписались от пользователя" });
    }

    /// <summary>
    /// Получить список подписчиков
    /// </summary>
    [HttpGet("followers")]
    public async Task<ActionResult<object>> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var followers = await _context.UserFollows
            .Include(f => f.Follower)
            .Where(f => f.FollowingId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                UserId = f.Follower.Id,
                UserName = f.Follower.UserName ?? f.Follower.Email,
                Email = f.Follower.Email,
                f.CreatedAt
            })
            .ToListAsync();

        var totalFollowers = await _context.UserFollows.CountAsync(f => f.FollowingId == userId);

        return Ok(new
        {
            TotalFollowers = totalFollowers,
            Page = page,
            PageSize = pageSize,
            Followers = followers
        });
    }

    /// <summary>
    /// Получить список подписок
    /// </summary>
    [HttpGet("following")]
    public async Task<ActionResult<object>> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var following = await _context.UserFollows
            .Include(f => f.Following)
            .Where(f => f.FollowerId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                UserId = f.Following.Id,
                UserName = f.Following.UserName ?? f.Following.Email,
                Email = f.Following.Email,
                f.CreatedAt
            })
            .ToListAsync();

        var totalFollowing = await _context.UserFollows.CountAsync(f => f.FollowerId == userId);

        return Ok(new
        {
            TotalFollowing = totalFollowing,
            Page = page,
            PageSize = pageSize,
            Following = following
        });
    }

    /// <summary>
    /// Получить ленту активности друзей
    /// </summary>
    [HttpGet("feed")]
    public async Task<ActionResult<object>> GetActivityFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        // Получаем ID пользователей, на которых подписан текущий пользователь
        var followingIds = await _context.UserFollows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        // Добавляем свою активность
        followingIds.Add(userId);

        var activities = await _context.ActivityFeeds
            .Include(a => a.User)
            .Where(a => followingIds.Contains(a.UserId))
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                User = new
                {
                    a.User.Id,
                    UserName = a.User.UserName ?? a.User.Email,
                    a.User.Email
                },
                a.ActivityType,
                a.Description,
                a.Metadata,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            Activities = activities
        });
    }

    /// <summary>
    /// Добавить активность в ленту (внутренний метод, вызывается при действиях пользователя)
    /// </summary>
    [HttpPost("activity")]
    public async Task<ActionResult> AddActivity([FromBody] CreateActivityDto dto)
    {
        var userId = GetUserId();

        var activity = new ActivityFeed
        {
            UserId = userId,
            ActivityType = dto.ActivityType,
            Description = dto.Description,
            Metadata = dto.Metadata
        };

        _context.ActivityFeeds.Add(activity);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Активность добавлена" });
    }

    /// <summary>
    /// Получить статистику пользователя (публичный профиль)
    /// </summary>
    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<object>> GetUserProfile(string userId)
    {
        var currentUserId = GetUserId();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        var followersCount = await _context.UserFollows.CountAsync(f => f.FollowingId == userId);
        var followingCount = await _context.UserFollows.CountAsync(f => f.FollowerId == userId);
        
        var isFollowing = await _context.UserFollows
            .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);

        var quizzesTaken = await _context.UserQuizAttempts
            .Where(qa => qa.UserId == userId)
            .Select(qa => qa.QuizId)
            .Distinct()
            .CountAsync();

        var flashcardSetsCreated = await _context.FlashcardSets
            .Where(fs => fs.UserId == userId)
            .CountAsync();

        var achievementsUnlocked = await _context.UserAchievements
            .Where(ua => ua.UserId == userId && ua.IsCompleted)
            .CountAsync();

        var streak = await _context.UserStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId);

        return Ok(new
        {
            User = new
            {
                user.Id,
                UserName = user.UserName ?? user.Email,
                user.Email,
                user.FirstName,
                user.LastName,
                user.CreatedAt
            },
            Stats = new
            {
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                QuizzesTaken = quizzesTaken,
                FlashcardSetsCreated = flashcardSetsCreated,
                AchievementsUnlocked = achievementsUnlocked,
                CurrentStreak = streak?.CurrentStreak ?? 0,
                LongestStreak = streak?.LongestStreak ?? 0
            },
            IsFollowing = isFollowing
        });
    }
}

// DTOs
public class CreateActivityDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}
