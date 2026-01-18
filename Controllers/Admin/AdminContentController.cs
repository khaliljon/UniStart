using Microsoft.AspNetCore.Authorization;
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
public class AdminContentController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminContentController> _logger;

    public AdminContentController(
        IUnitOfWork unitOfWork,
        ILogger<AdminContentController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Получить все квизы в системе (для админа) с фильтрацией
    /// </summary>
    [HttpGet("quizzes")]
    public async Task<ActionResult> GetAllQuizzes(
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null)
    {
        var query = _unitOfWork.Quizzes.Query()
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
                UserName = q.User != null ? q.User.UserName : "Unknown",
                QuestionCount = q.Questions.Count,
                MaxScore = q.Questions.Sum(qu => qu.Points),
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
        var flashcardSets = await _unitOfWork.FlashcardSets.Query()
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
                UserName = fs.User != null ? fs.User.UserName : "Unknown",
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
        var query = _unitOfWork.Exams.Query()
            .Include(e => e.Questions)
            .Include(e => e.User)
            .Include(e => e.Subjects)
            .AsQueryable();

        if (!string.IsNullOrEmpty(subject))
            query = query.Where(e => e.Subjects.Any(s => s.Name == subject));

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
                    _unitOfWork.Repository<University>().Query().Any(u => u.Id == e.UniversityId.Value && u.CountryId == countryId.Value));
            }
        }

        var exams = await query
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Description,
                e.Difficulty,
                e.IsPublished,
                e.UserId,
                UserName = e.User != null ? e.User.UserName : "Unknown",
                QuestionCount = e.Questions.Count,
                MaxScore = e.MaxScore,
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
    /// Получить все достижения (для админа)
    /// </summary>
    [HttpGet("achievements")]
    public async Task<ActionResult<object>> GetAllAchievements()
    {
        var achievements = await _unitOfWork.Achievements.Query()
            .OrderBy(a => a.Id)
            .Select(a => new
            {
                id = a.Id.ToString(),
                name = a.Title,
                description = a.Description,
                iconName = a.Icon,
                category = a.Type,
                requiredCount = a.TargetValue,
                createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            })
            .ToListAsync();

        return Ok(achievements);
    }
}
