using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Admin)] // Только администраторы
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminController> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех пользователей с пагинацией
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<object>> GetUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        [FromQuery] string? search = null)
    {
        var query = _userManager.Users.AsQueryable();

        // Поиск по email или username
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => 
                u.Email!.Contains(search) || 
                (u.UserName != null && u.UserName.Contains(search)));
        }

        var totalUsers = await query.CountAsync();
        
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Получаем роли для каждого пользователя
        var userDtos = new List<object>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Фильтр по роли (если указан)
            if (!string.IsNullOrWhiteSpace(role) && !userRoles.Contains(role))
                continue;

            userDtos.Add(new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.CreatedAt,
                user.TotalCardsStudied,
                user.TotalQuizzesTaken,
                Roles = userRoles,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd
            });
        }

        return Ok(new
        {
            TotalUsers = totalUsers,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize),
            Users = userDtos
        });
    }

    /// <summary>
    /// Получить детальную информацию о пользователе
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ActionResult<object>> GetUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        var roles = await _userManager.GetRolesAsync(user);

        // Статистика пользователя
        var flashcardSetsCount = await _context.FlashcardSets
            .Where(fs => fs.UserId == userId)
            .CountAsync();

        var quizzesCount = await _context.Quizzes
            .Where(q => q.UserId == userId)
            .CountAsync();

        var quizAttemptsCount = await _context.UserQuizAttempts
            .Where(qa => qa.UserId == userId)
            .CountAsync();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName,
            user.CreatedAt,
            user.TotalCardsStudied,
            user.TotalQuizzesTaken,
            Roles = roles,
            user.EmailConfirmed,
            user.LockoutEnabled,
            user.LockoutEnd,
            Statistics = new
            {
                FlashcardSetsCreated = flashcardSetsCount,
                QuizzesCreated = quizzesCount,
                QuizAttempts = quizAttemptsCount
            }
        });
    }

    /// <summary>
    /// Назначить роль пользователю
    /// </summary>
    [HttpPost("users/{userId}/role")]
    public async Task<ActionResult> AssignRole(string userId, [FromBody] AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        // Проверяем, что роль существует
        if (!await _roleManager.RoleExistsAsync(dto.RoleName))
            return BadRequest(new { Message = $"Роль '{dto.RoleName}' не существует" });

        // Проверяем, что пользователь ещё не имеет эту роль
        if (await _userManager.IsInRoleAsync(user, dto.RoleName))
            return BadRequest(new { Message = $"Пользователь уже имеет роль '{dto.RoleName}'" });

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        
        if (!result.Succeeded)
            return BadRequest(new { Message = "Не удалось назначить роль", Errors = result.Errors });

        _logger.LogInformation("Админ назначил роль '{Role}' пользователю {Email}", dto.RoleName, user.Email);

        return Ok(new { Message = $"Роль '{dto.RoleName}' успешно назначена пользователю {user.Email}" });
    }

    /// <summary>
    /// Удалить роль у пользователя
    /// </summary>
    [HttpDelete("users/{userId}/role")]
    public async Task<ActionResult> RemoveRole(string userId, [FromBody] AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        // Проверяем, что пользователь имеет эту роль
        if (!await _userManager.IsInRoleAsync(user, dto.RoleName))
            return BadRequest(new { Message = $"Пользователь не имеет роль '{dto.RoleName}'" });

        var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);
        
        if (!result.Succeeded)
            return BadRequest(new { Message = "Не удалось удалить роль", Errors = result.Errors });

        _logger.LogInformation("Админ удалил роль '{Role}' у пользователя {Email}", dto.RoleName, user.Email);

        return Ok(new { Message = $"Роль '{dto.RoleName}' успешно удалена у пользователя {user.Email}" });
    }

    /// <summary>
    /// Заблокировать/разблокировать пользователя
    /// </summary>
    [HttpPost("users/{userId}/lockout")]
    public async Task<ActionResult> SetLockout(string userId, [FromBody] SetLockoutDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        // Не даём заблокировать самого себя
        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
            return BadRequest(new { Message = "Нельзя заблокировать самого себя" });

        DateTimeOffset? lockoutEnd = dto.IsLocked ? DateTimeOffset.UtcNow.AddYears(100) : null;
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        
        if (!result.Succeeded)
            return BadRequest(new { Message = "Не удалось изменить статус блокировки", Errors = result.Errors });

        var status = dto.IsLocked ? "заблокирован" : "разблокирован";
        _logger.LogInformation("Пользователь {Email} {Status}", user.Email, status);

        return Ok(new { Message = $"Пользователь {user.Email} {status}" });
    }

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    [HttpDelete("users/{userId}")]
    public async Task<ActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        // Не даём удалить самого себя
        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
            return BadRequest(new { Message = "Нельзя удалить самого себя" });

        // Удаляем связанные данные
        var flashcardSets = await _context.FlashcardSets.Where(fs => fs.UserId == userId).ToListAsync();
        _context.FlashcardSets.RemoveRange(flashcardSets);

        var quizzes = await _context.Quizzes.Where(q => q.UserId == userId).ToListAsync();
        _context.Quizzes.RemoveRange(quizzes);

        var quizAttempts = await _context.UserQuizAttempts.Where(qa => qa.UserId == userId).ToListAsync();
        _context.UserQuizAttempts.RemoveRange(quizAttempts);

        await _context.SaveChangesAsync();

        // Удаляем пользователя
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { Message = "Не удалось удалить пользователя", Errors = result.Errors });

        _logger.LogWarning("Пользователь {Email} был удалён администратором", user.Email);

        return Ok(new { Message = $"Пользователь {user.Email} успешно удалён" });
    }

    /// <summary>
    /// Получить статистику платформы
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetPlatformStats()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalFlashcardSets = await _context.FlashcardSets.CountAsync();
        var totalFlashcards = await _context.Flashcards.CountAsync();
        var totalQuizzes = await _context.Quizzes.CountAsync();
        var totalQuizAttempts = await _context.UserQuizAttempts.CountAsync();

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

        var newQuizzesLastWeek = await _context.Quizzes
            .Where(q => q.CreatedAt >= sevenDaysAgo)
            .CountAsync();

        var quizAttemptsLastWeek = await _context.UserQuizAttempts
            .Where(qa => qa.CompletedAt >= sevenDaysAgo)
            .CountAsync();

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
    /// Получить список всех ролей
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<object>> GetRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        
        var roleDtos = new List<object>();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            roleDtos.Add(new
            {
                role.Id,
                role.Name,
                UserCount = usersInRole.Count
            });
        }

        return Ok(roleDtos);
    }

    /// <summary>
    /// Получить детальную аналитику платформы с графиками
    /// </summary>
    [HttpGet("analytics/detailed")]
    public async Task<ActionResult<object>> GetDetailedAnalytics([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // Активность по дням
        var dailyActivity = await _context.UserQuizAttempts
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
        var popularSubjects = await _context.Quizzes
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
        var topUsers = await _context.UserQuizAttempts
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
        var achievementStats = await _context.UserAchievements
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

        var hourlyActivity = await _context.UserQuizAttempts
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

    /// <summary>
    /// Экспорт полной статистики в CSV
    /// </summary>
    [HttpGet("export/full-stats")]
    public async Task<ActionResult> ExportFullStats()
    {
        var users = await _userManager.Users.ToListAsync();
        var csv = new System.Text.StringBuilder();
        
        csv.AppendLine("UserId,Email,UserName,CreatedAt,TotalCardsStudied,TotalQuizzesTaken,Roles");

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            csv.AppendLine($"{user.Id},{user.Email},{user.UserName},{user.CreatedAt:yyyy-MM-dd},{user.TotalCardsStudied},{user.TotalQuizzesTaken},\"{string.Join(";", roles)}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Users_Export_{DateTime.UtcNow:yyyyMMdd}.csv";

        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Получить все квизы в системе (для админа), исключая seed данные
    /// </summary>
    [HttpGet("quizzes")]
    public async Task<ActionResult> GetAllQuizzes()
    {
        // Получаем ID тестового пользователя (seed user)
        var seedUser = await _userManager.FindByEmailAsync("test@unistart.kz");
        var seedUserId = seedUser?.Id;

        var quizzes = await _context.Quizzes
            .Include(q => q.Questions)
            .Where(q => seedUserId == null || q.UserId != seedUserId) // Исключаем seed квизы
            .Select(q => new
            {
                q.Id,
                q.Title,
                q.Description,
                q.Subject,
                q.Difficulty,
                q.IsPublished,
                q.UserId,
                QuestionCount = q.Questions.Count,
                TotalPoints = q.Questions.Sum(qu => qu.Points),
                q.CreatedAt
            })
            .ToListAsync();

        return Ok(quizzes);
    }

    /// <summary>
    /// Получить все наборы карточек в системе (для админа), исключая seed данные
    /// </summary>
    [HttpGet("flashcards")]
    public async Task<ActionResult> GetAllFlashcardSets()
    {
        // Получаем ID тестового пользователя (seed user)
        var seedUser = await _userManager.FindByEmailAsync("test@unistart.kz");
        var seedUserId = seedUser?.Id;

        var flashcardSets = await _context.FlashcardSets
            .Include(fs => fs.Flashcards)
            .Where(fs => seedUserId == null || fs.UserId != seedUserId) // Исключаем seed карточки
            .Select(fs => new
            {
                fs.Id,
                fs.Title,
                fs.Description,
                fs.Subject,
                fs.IsPublic,
                fs.UserId,
                CardCount = fs.Flashcards.Count,
                fs.CreatedAt
            })
            .ToListAsync();

        return Ok(flashcardSets);
    }
}

// DTOs для AdminController
public class AssignRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}

public class SetLockoutDto
{
    public bool IsLocked { get; set; }
}
