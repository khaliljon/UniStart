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
public class AdminUsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminUsersController> logger)
    {
        _unitOfWork = unitOfWork;
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

        var userIds = users.Select(u => u.Id).ToList();

        // ===== ОПТИМИЗАЦИЯ: Получаем все данные групповыми запросами =====
        
        // 1. Получаем все попытки квизов одним запросом
        var allQuizAttempts = await _unitOfWork.QuizAttempts.Query()
            .Where(qa => userIds.Contains(qa.UserId) && qa.CompletedAt != null)
            .ToListAsync();

        var quizStatsDict = allQuizAttempts
            .GroupBy(qa => qa.UserId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalAttempts = g.Count(),
                    UniqueQuizzes = g.Select(qa => qa.QuizId).Distinct().Count(),
                    AverageScore = Math.Round(g.Average(qa => qa.Percentage), 2),
                    LastQuizDate = g.Max(qa => qa.CompletedAt)
                });

        // 2. Получаем все попытки экзаменов одним запросом
        var allExamAttempts = await _unitOfWork.ExamAttempts.Query()
            .Where(ea => userIds.Contains(ea.UserId) && ea.CompletedAt != null)
            .ToListAsync();

        var examStatsDict = allExamAttempts
            .GroupBy(ea => ea.UserId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalExams = g.Select(ea => ea.ExamId).Distinct().Count(),
                    AverageScore = Math.Round(g.Average(ea => ea.Percentage), 2),
                    LastExamDate = g.Max(ea => ea.CompletedAt)
                });

        // 3. Получаем статистику по наборам карточек одним запросом
        var allFlashcardSetAccesses = await _unitOfWork.Repository<UserFlashcardSetAccess>().Query()
            .Where(a => userIds.Contains(a.UserId))
            .ToListAsync();

        var flashcardSetsDict = allFlashcardSetAccesses
            .GroupBy(a => a.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Count(a => a.IsCompleted));

        // 4. Получаем прогресс по карточкам одним запросом
        var allFlashcardProgresses = await _unitOfWork.FlashcardProgress.Query()
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync();

        var flashcardProgressDict = allFlashcardProgresses
            .GroupBy(p => p.UserId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    ReviewedCards = g.Count(p => p.LastReviewedAt != null),
                    MasteredCards = g.Count(p => p.IsMastered),
                    LastCardDate = g.Where(p => p.LastReviewedAt != null)
                        .Select(p => (DateTime?)p.LastReviewedAt)
                        .DefaultIfEmpty()
                        .Max()
                });

        // Формируем результат для каждого пользователя
        var userDtos = new List<object>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Фильтр по роли (если указан)
            if (!string.IsNullOrWhiteSpace(role) && !userRoles.Contains(role))
                continue;

            // Получаем статистику из словарей (без дополнительных запросов!)
            var quizStats = quizStatsDict.TryGetValue(user.Id, out var qs) 
                ? qs 
                : new { TotalAttempts = 0, UniqueQuizzes = 0, AverageScore = 0.0, LastQuizDate = (DateTime?)null };

            var examStats = examStatsDict.TryGetValue(user.Id, out var es) 
                ? es 
                : new { TotalExams = 0, AverageScore = 0.0, LastExamDate = (DateTime?)null };

            var completedSets = flashcardSetsDict.TryGetValue(user.Id, out var cs) ? cs : 0;

            var flashcardProgress = flashcardProgressDict.TryGetValue(user.Id, out var fp) 
                ? fp 
                : new { ReviewedCards = 0, MasteredCards = 0, LastCardDate = (DateTime?)null };

            // Вычисляем последнюю активность
            var lastActivityDate = new[] { quizStats.LastQuizDate, examStats.LastExamDate, flashcardProgress.LastCardDate }
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
                LastActivityDate = lastActivityDate,
                
                // Статистика по карточкам
                CompletedFlashcardSets = completedSets,
                ReviewedCards = flashcardProgress.ReviewedCards,
                MasteredCards = flashcardProgress.MasteredCards,
                
                // Статистика по квизам
                TotalQuizzesTaken = quizStats.UniqueQuizzes,
                TotalQuizAttempts = quizStats.TotalAttempts,
                AverageScore = quizStats.AverageScore,
                
                // Статистика по экзаменам
                TotalExamsTaken = examStats.TotalExams,
                AverageExamScore = examStats.AverageScore,
                
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
        var flashcardSetsCount = await _unitOfWork.FlashcardSets.CountAsync(fs => fs.UserId == userId);
        var quizzesCount = await _unitOfWork.Quizzes.CountAsync(q => q.UserId == userId);
        var quizAttemptsCount = await _unitOfWork.QuizAttempts.CountAsync(qa => qa.UserId == userId);

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

        // Удаляем связанные данные через репозитории
        var flashcardSets = await _unitOfWork.FlashcardSets.FindAsync(fs => fs.UserId == userId);
        _unitOfWork.FlashcardSets.RemoveRange(flashcardSets);

        var quizzes = await _unitOfWork.Quizzes.FindAsync(q => q.UserId == userId);
        _unitOfWork.Quizzes.RemoveRange(quizzes);

        var quizAttempts = await _unitOfWork.QuizAttempts.FindAsync(qa => qa.UserId == userId);
        _unitOfWork.QuizAttempts.RemoveRange(quizAttempts);

        await _unitOfWork.SaveChangesAsync();

        // Удаляем пользователя
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { Message = "Не удалось удалить пользователя", Errors = result.Errors });

        _logger.LogWarning("Пользователь {Email} был удалён администратором", user.Email);

        return Ok(new { Message = $"Пользователь {user.Email} успешно удалён" });
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
        var quizAttempts = await _unitOfWork.QuizAttempts.Query()
            .Where(qa => qa.UserId == studentId && qa.CompletedAt != null)
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();

        // Получаем попытки студента по всем экзаменам (без Include для избежания дублирования)
        var examAttempts = await _unitOfWork.ExamAttempts.Query()
            .Where(ea => ea.UserId == studentId && ea.CompletedAt != null)
            .OrderByDescending(ea => ea.CompletedAt)
            .ToListAsync();

        // Статистика по карточкам
        var completedSetsCount = await _unitOfWork.Repository<UserFlashcardSetAccess>().CountAsync(
            a => a.UserId == studentId && a.IsCompleted);
        
        var reviewedCardsCount = await _unitOfWork.FlashcardProgress.CountAsync(
            p => p.UserId == studentId && p.LastReviewedAt != null);
        
        var masteredCardsCount = await _unitOfWork.FlashcardProgress.CountAsync(
            p => p.UserId == studentId && p.IsMastered);

        // Получаем информацию о квизах и экзаменах отдельно для отображения
        var quizIds = quizAttempts.Select(a => a.QuizId).Distinct().ToList();
        var examIds = examAttempts.Select(ea => ea.ExamId).Distinct().ToList();
        
        var quizzesDict = await _unitOfWork.Quizzes.Query()
            .Where(q => quizIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);
        
        var examsDict = await _unitOfWork.Exams.Query()
            .Where(e => examIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        // Получаем детальную информацию о наборах карточек
        var flashcardSetsAccess = await _unitOfWork.Repository<UserFlashcardSetAccess>().Query()
            .Where(a => a.UserId == studentId)
            .Include(a => a.FlashcardSet)
            .OrderByDescending(a => a.LastAccessedAt)
            .ToListAsync();

        // ОПТИМИЗИРОВАНО: Получаем прогресс по карточкам через Join вместо Include
        var flashcardProgressBySet = await (
            from progress in _unitOfWork.FlashcardProgress.Query()
            join flashcard in _unitOfWork.Repository<Flashcard>().Query() on progress.FlashcardId equals flashcard.Id
            where progress.UserId == studentId
            group progress by flashcard.FlashcardSetId into g
            select new
            {
                SetId = g.Key,
                ReviewedCards = g.Count(p => p.LastReviewedAt != null),
                MasteredCards = g.Count(p => p.IsMastered)
            }
        ).ToDictionaryAsync(x => x.SetId);

        var flashcardSetDetails = flashcardSetsAccess.Select(a =>
        {
            var hasProgress = flashcardProgressBySet.TryGetValue(a.FlashcardSetId, out var progressStats);
            var reviewedCards = hasProgress ? progressStats.ReviewedCards : 0;
            var masteredCards = hasProgress ? progressStats.MasteredCards : 0;
            
            return new
            {
                SetId = a.FlashcardSetId,
                SetTitle = a.FlashcardSet.Title,
                TotalCards = a.TotalCardsCount,
                ReviewedCards = reviewedCards,
                MasteredCards = masteredCards,
                IsCompleted = a.IsCompleted,
                LastAccessed = a.LastAccessedAt ?? a.FirstAccessedAt
            };
        }).ToList();

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

        // Правильно считаем средний балл
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
            
            // Статистика по карточкам
            CompletedFlashcardSets = completedSetsCount,
            ReviewedCards = reviewedCardsCount,
            MasteredCards = masteredCardsCount,
            
            // Статистика по квизам
            TotalQuizAttempts = quizAttempts.Count,
            QuizzesTaken = quizAttempts.Select(a => a.QuizId).Distinct().Count(),
            AverageQuizScore = quizAttempts.Any() ? Math.Round(quizAttempts.Average(a => a.Percentage), 2) : 0.0,
            BestQuizScore = quizAttempts.Any() ? quizAttempts.Max(a => a.Percentage) : 0.0,
            
            // Статистика по экзаменам
            TotalExamAttempts = examAttempts.Count,
            ExamsTaken = examAttempts.Select(ea => ea.ExamId).Distinct().Count(),
            AverageExamScore = examAttempts.Any() ? Math.Round(examAttempts.Average(ea => ea.Percentage), 2) : 0.0,
            BestExamScore = examAttempts.Any() ? examAttempts.Max(ea => ea.Percentage) : 0.0,
            
            // Общая статистика
            AverageScore = overallAverage,
            Attempts = attemptDetails,
            ExamAttempts = examAttemptDetails,
            
            // Детальная статистика по карточкам
            FlashcardProgress = new
            {
                SetsAccessed = flashcardSetsAccess.Count,
                SetsCompleted = completedSetsCount,
                TotalCardsReviewed = reviewedCardsCount,
                MasteredCards = masteredCardsCount,
                SetDetails = flashcardSetDetails
            }
        });
    }
}

// DTOs
public class AssignRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}

public class SetLockoutDto
{
    public bool IsLocked { get; set; }
}
