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

            // Получаем статистику по квизам для пользователя
            var quizAttempts = await _context.UserQuizAttempts
                .Where(qa => qa.UserId == user.Id && qa.CompletedAt != null)
                .ToListAsync();

            var totalQuizAttempts = quizAttempts.Count; // Реальное количество попыток
            var uniqueQuizzesTaken = quizAttempts.Select(qa => qa.QuizId).Distinct().Count(); // Уникальные квизы
            var averageScore = quizAttempts.Any() 
                ? Math.Round(quizAttempts.Average(qa => qa.Percentage), 2) 
                : 0.0;

            // Получаем статистику по экзаменам
            var examAttempts = await _context.UserExamAttempts
                .Where(ea => ea.UserId == user.Id && ea.CompletedAt != null)
                .ToListAsync();

            var totalExamsTaken = examAttempts.Select(ea => ea.ExamId).Distinct().Count();
            var averageExamScore = examAttempts.Any()
                ? Math.Round(examAttempts.Average(ea => ea.Percentage), 2)
                : 0.0;

            // Получаем количество полностью изученных наборов карточек (где все карточки освоены пользователем)
            // Используем новую модель UserFlashcardSetAccess для точного подсчета
            var studiedSetsCount = await _context.UserFlashcardSetAccesses
                .Where(a => a.UserId == user.Id && a.IsCompleted)
                .CountAsync();
            
            var studiedCardsCount = studiedSetsCount; // Это количество полностью изученных наборов

            // Получаем дату последней активности (максимум из всех видов активности)
            var lastQuizDate = quizAttempts.Any() ? quizAttempts.Max(qa => qa.CompletedAt) : (DateTime?)null;
            var lastExamDate = examAttempts.Any() ? examAttempts.Max(ea => ea.CompletedAt) : (DateTime?)null;
            
            // Для карточек ищем максимальную дату LastReviewedAt из UserFlashcardProgress (индивидуальный прогресс)
            var lastCardDate = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == user.Id && p.LastReviewedAt != null)
                .Select(p => (DateTime?)p.LastReviewedAt)
                .DefaultIfEmpty()
                .MaxAsync();

            var lastActivityDate = new[] { lastQuizDate, lastExamDate, lastCardDate }
                .Where(d => d.HasValue)
                .DefaultIfEmpty()
                .Max();

            userDtos.Add(new
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                LastActivityDate = lastActivityDate, // Дата последней активности (квизы, экзамены, карточки)
                TotalCardsStudied = studiedCardsCount, // Количество полностью изученных наборов карточек
                TotalQuizzesTaken = uniqueQuizzesTaken, // Уникальные квизы (не попытки!)
                TotalQuizAttempts = totalQuizAttempts, // Реальное количество попыток по квизам
                TotalExamsTaken = totalExamsTaken,
                AverageScore = averageScore, // Средний процент по всем попыткам квизов
                AverageExamScore = averageExamScore,
                Roles = userRoles,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
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
    /// Получить основную аналитику платформы
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<object>> GetAnalytics()
    {
        // Используем _userManager.Users для подсчета пользователей Identity
        var totalUsers = await _userManager.Users.CountAsync();
        var totalQuizzes = await _context.Quizzes.CountAsync();
        var totalExams = await _context.Exams.CountAsync();
        var totalFlashcardSets = await _context.FlashcardSets.CountAsync();
        var totalQuestions = await _context.QuizQuestions.CountAsync();
        var totalFlashcards = await _context.Flashcards.CountAsync();
        var totalAttempts = await _context.UserQuizAttempts.CountAsync();
        
        var today = DateTime.UtcNow.Date;
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var monthAgo = DateTime.UtcNow.AddMonths(-1);
        
        // Используем _userManager.Users для активных пользователей
        var activeToday = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date >= today)
            .CountAsync();
            
        var activeThisWeek = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date >= weekAgo)
            .CountAsync();
            
        var activeThisMonth = await _userManager.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date >= monthAgo)
            .CountAsync();
        
        // Получаем средний балл только по завершенным попыткам
        var completedQuizAttempts = await _context.UserQuizAttempts
            .Where(qa => qa.CompletedAt != null)
            .ToListAsync();
        
        var averageQuizScore = completedQuizAttempts.Any() 
            ? completedQuizAttempts.Average(qa => qa.Percentage)
            : 0.0;
            
        var totalAchievements = await _context.Achievements.CountAsync();
        
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
    /// Экспорт пользователей в CSV
    /// </summary>
    [HttpGet("export/users")]
    public async Task<ActionResult> ExportUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var csv = new System.Text.StringBuilder();
        
        csv.AppendLine("Email,UserName,FirstName,LastName,CreatedAt,LastLoginAt,TotalCardsStudied,TotalQuizzesTaken,Roles");

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            csv.AppendLine($"{user.Email},{user.UserName},{user.FirstName},{user.LastName},{user.CreatedAt:yyyy-MM-dd HH:mm},{user.LastLoginAt:yyyy-MM-dd HH:mm},{user.TotalCardsStudied},{user.TotalQuizzesTaken},\"{string.Join(";", roles)}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Users_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Экспорт квизов в CSV
    /// </summary>
    [HttpGet("export/quizzes")]
    public async Task<ActionResult> ExportQuizzes()
    {
        var quizzes = await _context.Quizzes
            .Include(q => q.Questions)
            .Include(q => q.User)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("QuizId,Title,Subject,Difficulty,CreatedBy,QuestionCount,TotalPoints,IsPublished,CreatedAt");

        foreach (var quiz in quizzes)
        {
            csv.AppendLine($"{quiz.Id},\"{quiz.Title}\",{quiz.Subject},{quiz.Difficulty},{quiz.User.UserName},{quiz.Questions.Count},{quiz.Questions.Sum(q => q.Points)},{quiz.IsPublished},{quiz.CreatedAt:yyyy-MM-dd}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Quizzes_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Экспорт наборов карточек в CSV
    /// </summary>
    [HttpGet("export/flashcards")]
    public async Task<ActionResult> ExportFlashcards()
    {
        var sets = await _context.FlashcardSets
            .Include(fs => fs.Flashcards)
            .Include(fs => fs.User)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("SetId,Title,Subject,CreatedBy,CardCount,IsPublic,CreatedAt");

        foreach (var set in sets)
        {
            csv.AppendLine($"{set.Id},\"{set.Title}\",{set.Subject},{set.User.UserName},{set.Flashcards.Count},{set.IsPublic},{set.CreatedAt:yyyy-MM-dd}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Flashcards_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Экспорт попыток тестов в CSV
    /// </summary>
    [HttpGet("export/attempts")]
    public async Task<ActionResult> ExportAttempts()
    {
        var attempts = await _context.UserQuizAttempts
            .Include(qa => qa.User)
            .Include(qa => qa.Quiz)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("AttemptId,User,Quiz,Score,MaxScore,Percentage,TimeSpent,CompletedAt");

        foreach (var attempt in attempts)
        {
            csv.AppendLine($"{attempt.Id},{attempt.User.Email},\"{attempt.Quiz.Title}\",{attempt.Score},{attempt.MaxScore},{attempt.Percentage:F2},{attempt.TimeSpentSeconds},{attempt.CompletedAt:yyyy-MM-dd HH:mm}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Attempts_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Получить все квизы в системе (для админа) с фильтрацией
    /// </summary>
    [HttpGet("quizzes")]
    public async Task<ActionResult> GetAllQuizzes(
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null)
    {
        var query = _context.Quizzes
            .Include(q => q.Questions)
            .Include(q => q.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(subject))
            query = query.Where(q => q.Subject == subject);

        if (!string.IsNullOrEmpty(difficulty))
            query = query.Where(q => q.Difficulty == difficulty);

        var quizzes = await query
            .Select(q => new
            {
                q.Id,
                q.Title,
                q.Description,
                q.Subject,
                q.Difficulty,
                q.TimeLimit,
                q.IsPublished,
                q.UserId,
                UserName = q.User.UserName,
                QuestionCount = q.Questions.Count,
                TotalPoints = q.Questions.Sum(qu => qu.Points),
                q.CreatedAt
            })
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        return Ok(quizzes);
    }

    /// <summary>
    /// Получить все наборы карточек в системе (для админа)
    /// </summary>
    [HttpGet("flashcards")]
    public async Task<ActionResult> GetAllFlashcardSets()
    {
        var flashcardSets = await _context.FlashcardSets
            .Include(fs => fs.Flashcards)
            .Include(fs => fs.User)
            .Select(fs => new
            {
                fs.Id,
                fs.Title,
                fs.Description,
                fs.Subject,
                fs.IsPublic,
                fs.UserId,
                UserName = fs.User.UserName,
                CardCount = fs.Flashcards.Count,
                fs.CreatedAt
            })
            .OrderByDescending(fs => fs.CreatedAt)
            .ToListAsync();

        return Ok(flashcardSets);
    }

    /// <summary>
    /// Получить все экзамены в системе (для админа) с фильтрацией
    /// </summary>
    [HttpGet("exams")]
    public async Task<ActionResult> GetAllExams(
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? country = null,
        [FromQuery] string? university = null)
    {
        var query = _context.Exams
            .Include(e => e.Questions)
            .Include(e => e.User)
            .Include(e => e.Subjects)
            .AsQueryable();

        if (!string.IsNullOrEmpty(subject))
            query = query.Where(e => e.Subjects.Any(s => s.Name == subject) || e.Subject == subject);

        if (!string.IsNullOrEmpty(difficulty))
            query = query.Where(e => e.Difficulty == difficulty);

        if (!string.IsNullOrEmpty(university))
        {
            var universityId = int.TryParse(university, out var uid) ? uid : (int?)null;
            if (universityId.HasValue)
                query = query.Where(e => e.UniversityId == universityId.Value);
        }

        if (!string.IsNullOrEmpty(country))
        {
            var countryId = int.TryParse(country, out var cid) ? cid : (int?)null;
            if (countryId.HasValue)
            {
                // Фильтруем по стране через университеты
                query = query.Where(e => e.UniversityId.HasValue && 
                    _context.Universities.Any(u => u.Id == e.UniversityId.Value && u.CountryId == countryId.Value));
            }
        }

        var exams = await query
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Description,
                e.Subject,
                e.Difficulty,
                e.IsPublished,
                e.UserId,
                UserName = e.User.UserName,
                QuestionCount = e.Questions.Count,
                e.TotalPoints,
                e.MaxAttempts,
                e.PassingScore,
                e.CreatedAt,
                e.UniversityId
            })
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return Ok(exams);
    }

    /// <summary>
    /// Получить настройки системы
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<object>> GetSettings()
    {
        var settings = await _context.SystemSettings.FindAsync(1);
        
        if (settings == null)
        {
            // Создаем настройки по умолчанию, если их нет
            settings = new SystemSettings
            {
                Id = 1,
                SiteName = "UniStart",
                SiteDescription = "Образовательная платформа для изучения с помощью карточек и тестов",
                AllowRegistration = true,
                RequireEmailVerification = false,
                MaxQuizAttempts = 3,
                SessionTimeout = 30,
                EnableNotifications = true,
                UpdatedAt = DateTime.UtcNow
            };
            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        
        return Ok(new
        {
            SiteName = settings.SiteName,
            SiteDescription = settings.SiteDescription,
            AllowRegistration = settings.AllowRegistration,
            RequireEmailVerification = settings.RequireEmailVerification,
            MaxQuizAttempts = settings.MaxQuizAttempts,
            SessionTimeout = settings.SessionTimeout,
            EnableNotifications = settings.EnableNotifications
        });
    }

    /// <summary>
    /// Обновить настройки системы
    /// </summary>
    [HttpPut("settings")]
    public async Task<ActionResult> UpdateSettings([FromBody] SystemSettingsDto dto)
    {
        try
        {
            // Валидация
            if (dto.SiteName != null && string.IsNullOrWhiteSpace(dto.SiteName))
                return BadRequest(new { message = "Название сайта не может быть пустым" });

            if (dto.MaxQuizAttempts.HasValue && (dto.MaxQuizAttempts.Value < 1 || dto.MaxQuizAttempts.Value > 10))
                return BadRequest(new { message = "Количество попыток должно быть от 1 до 10" });

            if (dto.SessionTimeout.HasValue && (dto.SessionTimeout.Value < 5 || dto.SessionTimeout.Value > 120))
                return BadRequest(new { message = "Таймаут сессии должен быть от 5 до 120 минут" });

            // Получаем или создаем настройки
            var settings = await _context.SystemSettings.FindAsync(1);
            if (settings == null)
            {
                settings = new SystemSettings
                {
                    Id = 1,
                    SiteName = "UniStart",
                    SiteDescription = "Образовательная платформа для изучения с помощью карточек и тестов",
                    AllowRegistration = true,
                    RequireEmailVerification = false,
                    MaxQuizAttempts = 3,
                    SessionTimeout = 30,
                    EnableNotifications = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SystemSettings.Add(settings);
            }
            
            // Обновляем только переданные значения
            if (dto.SiteName != null) settings.SiteName = dto.SiteName;
            if (dto.SiteDescription != null) settings.SiteDescription = dto.SiteDescription;
            if (dto.AllowRegistration.HasValue) settings.AllowRegistration = dto.AllowRegistration.Value;
            if (dto.RequireEmailVerification.HasValue) settings.RequireEmailVerification = dto.RequireEmailVerification.Value;
            if (dto.MaxQuizAttempts.HasValue) settings.MaxQuizAttempts = dto.MaxQuizAttempts.Value;
            if (dto.SessionTimeout.HasValue) settings.SessionTimeout = dto.SessionTimeout.Value;
            if (dto.EnableNotifications.HasValue) settings.EnableNotifications = dto.EnableNotifications.Value;
            
            settings.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Настройки системы обновлены: SiteName={SiteName}, SiteDescription={SiteDescription}, AllowRegistration={AllowRegistration}, MaxQuizAttempts={MaxQuizAttempts}, SessionTimeout={SessionTimeout}, EnableNotifications={EnableNotifications}",
                settings.SiteName,
                settings.SiteDescription,
                settings.AllowRegistration,
                settings.MaxQuizAttempts,
                settings.SessionTimeout,
                settings.EnableNotifications);
            
            return Ok(new { message = "Настройки успешно сохранены" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении настроек");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера при сохранении настроек" });
        }
    }

    /// <summary>
    /// Получить все достижения (для админа)
    /// </summary>
    [HttpGet("achievements")]
    public async Task<ActionResult<object>> GetAllAchievements()
    {
        var achievements = await _context.Achievements
            .OrderBy(a => a.Id)
            .Select(a => new
            {
                id = a.Id.ToString(),
                name = a.Title,
                description = a.Description,
                iconName = a.Icon,
                category = a.Type,
                requiredCount = a.TargetValue,
                createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") // Используем текущую дату, если нет CreatedAt
            })
            .ToListAsync();

            return Ok(achievements);
    }

    /// <summary>
    /// Получить детальную статистику по студенту (для админа)
    /// </summary>
    [HttpGet("students/{studentId}/stats")]
    public async Task<ActionResult<object>> GetStudentStats(string studentId)
    {
        // Получаем информацию о студенте
        var student = await _userManager.FindByIdAsync(studentId);
        if (student == null)
            return NotFound(new { Message = "Студент не найден" });

        // Получаем попытки студента по всем квизам (без Include для избежания дублирования)
        var quizAttempts = await _context.UserQuizAttempts
            .Where(qa => qa.UserId == studentId && qa.CompletedAt != null)
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();

        // Получаем попытки студента по всем экзаменам (без Include для избежания дублирования)
        var examAttempts = await _context.UserExamAttempts
            .Where(ea => ea.UserId == studentId && ea.CompletedAt != null)
            .OrderByDescending(ea => ea.CompletedAt)
            .ToListAsync();

        // Получаем количество полностью изученных наборов карточек (где все карточки освоены пользователем)
        // Используем новую модель UserFlashcardSetAccess для точного подсчета
        var studiedSetsCount = await _context.UserFlashcardSetAccesses
            .Where(a => a.UserId == studentId && a.IsCompleted)
            .CountAsync();
        
        var studiedCards = studiedSetsCount; // Это количество полностью изученных наборов

        // Получаем информацию о квизах и экзаменах отдельно для отображения
        var quizIds = quizAttempts.Select(a => a.QuizId).Distinct().ToList();
        var examIds = examAttempts.Select(ea => ea.ExamId).Distinct().ToList();
        
        var quizzesDict = await _context.Quizzes
            .Where(q => quizIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);
        
        var examsDict = await _context.Exams
            .Where(e => examIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        var attemptDetails = quizAttempts.Select(a => new
        {
            AttemptId = a.Id,
            QuizId = a.QuizId,
            QuizTitle = quizzesDict.TryGetValue(a.QuizId, out var quiz) ? quiz.Title : "Unknown",
            Type = "Quiz",
            Score = a.Score,
            MaxScore = a.MaxScore,
            Percentage = Math.Round(a.Percentage, 2),
            CompletedAt = a.CompletedAt
        }).ToList();

        var examAttemptDetails = examAttempts.Select(ea => new
        {
            AttemptId = ea.Id,
            ExamId = ea.ExamId,
            ExamTitle = examsDict.TryGetValue(ea.ExamId, out var exam) ? exam.Title : "Unknown",
            Type = "Exam",
            Score = ea.Score,
            TotalPoints = ea.TotalPoints,
            Percentage = Math.Round(ea.Percentage, 2),
            Passed = ea.Passed,
            CompletedAt = ea.CompletedAt
        }).ToList();

        // Правильно считаем средний балл - используем реальное количество попыток
        var totalQuizPercentage = quizAttempts.Sum(a => a.Percentage);
        var totalExamPercentage = examAttempts.Sum(ea => ea.Percentage);
        var totalAttemptsCount = quizAttempts.Count + examAttempts.Count;
        var overallAverage = totalAttemptsCount > 0 
            ? Math.Round((totalQuizPercentage + totalExamPercentage) / totalAttemptsCount, 2) 
            : 0.0;

        return Ok(new
        {
            StudentId = studentId,
            Email = student.Email,
            UserName = student.UserName,
            FirstName = student.FirstName,
            LastName = student.LastName,
            TotalQuizAttempts = quizAttempts.Count, // Реальное количество попыток
            TotalExamAttempts = examAttempts.Count,
            TotalCardsStudied = studiedCards, // Количество полностью изученных наборов карточек
            QuizzesTaken = quizAttempts.Select(a => a.QuizId).Distinct().Count(), // Уникальные квизы
            ExamsTaken = examAttempts.Select(ea => ea.ExamId).Distinct().Count(),
            AverageQuizScore = quizAttempts.Any() ? Math.Round(quizAttempts.Average(a => a.Percentage), 2) : 0.0,
            AverageExamScore = examAttempts.Any() ? Math.Round(examAttempts.Average(ea => ea.Percentage), 2) : 0.0,
            AverageScore = overallAverage, // Правильно рассчитанный общий средний балл
            BestQuizScore = quizAttempts.Any() ? quizAttempts.Max(a => a.Percentage) : 0.0,
            BestExamScore = examAttempts.Any() ? examAttempts.Max(ea => ea.Percentage) : 0.0,
            Attempts = attemptDetails,
            ExamAttempts = examAttemptDetails
        });
    }
}

// DTOs для AdminController

public class SystemSettingsDto
{
    public string? SiteName { get; set; }
    public string? SiteDescription { get; set; }
    public bool? AllowRegistration { get; set; }
    public bool? RequireEmailVerification { get; set; }
    public int? MaxQuizAttempts { get; set; }
    public int? SessionTimeout { get; set; }
    public bool? EnableNotifications { get; set; }
}
public class AssignRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}

public class SetLockoutDto
{
    public bool IsLocked { get; set; }
}
