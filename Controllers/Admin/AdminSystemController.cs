using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
public class AdminSystemController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminSystemController> _logger;

    public AdminSystemController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminSystemController> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Получить настройки системы
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<object>> GetSettings()
    {
        var settings = await _unitOfWork.Repository<SystemSettings>().GetByIdAsync(1);
        
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
            await _unitOfWork.Repository<SystemSettings>().AddAsync(settings);
            await _unitOfWork.SaveChangesAsync();
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
            var settings = await _unitOfWork.Repository<SystemSettings>().GetByIdAsync(1);
            if (settings == null)
            {
                settings = new SystemSettings
                {
                    Id = 1,
                    SiteName = "UniStart",
                    SiteDescription = "Образовательная платформа для изучения с помощью карточек и квизов",
                    AllowRegistration = true,
                    RequireEmailVerification = false,
                    MaxQuizAttempts = 3,
                    SessionTimeout = 30,
                    EnableNotifications = true,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<SystemSettings>().AddAsync(settings);
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
            
            await _unitOfWork.SaveChangesAsync();
            
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
    /// Экспорт полной статистики в CSV
    /// </summary>
    [HttpGet("export/full-stats")]
    public async Task<ActionResult> ExportFullStats()
    {
        var users = await _userManager.Users.ToListAsync();
        var csv = new StringBuilder();
        
        csv.AppendLine("UserId,Email,UserName,CreatedAt,TotalCardsStudied,TotalQuizzesTaken,Roles");

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            csv.AppendLine($"{user.Id},{user.Email},{user.UserName},{user.CreatedAt:yyyy-MM-dd},{user.TotalCardsStudied},{user.TotalQuizzesTaken},\"{string.Join(";", roles)}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
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
        var csv = new StringBuilder();
        
        csv.AppendLine("Email,UserName,FirstName,LastName,CreatedAt,LastLoginAt,TotalCardsStudied,TotalQuizzesTaken,Roles");

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            csv.AppendLine($"{user.Email},{user.UserName},{user.FirstName},{user.LastName},{user.CreatedAt:yyyy-MM-dd HH:mm},{user.LastLoginAt:yyyy-MM-dd HH:mm},{user.TotalCardsStudied},{user.TotalQuizzesTaken},\"{string.Join(";", roles)}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Users_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Экспорт квизов в CSV
    /// </summary>
    [HttpGet("export/quizzes")]
    public async Task<ActionResult> ExportQuizzes()
    {
        var quizzes = await _unitOfWork.Quizzes.Query()
            .Include(q => q.Questions)
            .Include(q => q.User)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("QuizId,Title,Subject,Difficulty,CreatedBy,QuestionCount,TotalPoints,IsPublished,CreatedAt");

        foreach (var quiz in quizzes)
        {
            var userName = quiz.User?.UserName ?? "Unknown";
            csv.AppendLine($"{quiz.Id},\"{quiz.Title}\",{quiz.Subject},{quiz.Difficulty},{userName},{quiz.Questions.Count},{quiz.Questions.Sum(q => q.Points)},{quiz.IsPublished},{quiz.CreatedAt:yyyy-MM-dd}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Quizzes_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Экспорт наборов карточек в CSV
    /// </summary>
    [HttpGet("export/flashcards")]
    public async Task<ActionResult> ExportFlashcards()
    {
        var sets = await _unitOfWork.FlashcardSets.Query()
            .Include(fs => fs.Flashcards)
            .Include(fs => fs.User)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("SetId,Title,Subject,CreatedBy,CardCount,IsPublic,CreatedAt");

        foreach (var set in sets)
        {
            var userName = set.User?.UserName ?? "Unknown";
            csv.AppendLine($"{set.Id},\"{set.Title}\",{set.Subject},{userName},{set.Flashcards.Count},{set.IsPublic},{set.CreatedAt:yyyy-MM-dd}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Flashcards_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Экспорт попыток квизов в CSV
    /// </summary>
    [HttpGet("export/attempts")]
    public async Task<ActionResult> ExportAttempts()
    {
        var attempts = await _unitOfWork.QuizAttempts.Query()
            .Include(qa => qa.User)
            .Include(qa => qa.Quiz)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("AttemptId,User,Quiz,Score,MaxScore,Percentage,TimeSpent,CompletedAt");

        foreach (var attempt in attempts)
        {
            csv.AppendLine($"{attempt.Id},{attempt.User.Email},\"{attempt.Quiz.Title}\",{attempt.Score},{attempt.MaxScore},{attempt.Percentage:F2},{attempt.TimeSpentSeconds},{attempt.CompletedAt:yyyy-MM-dd HH:mm}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"UniStart_Attempts_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }
}

// DTOs
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
