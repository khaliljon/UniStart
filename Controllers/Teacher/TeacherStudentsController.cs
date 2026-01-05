using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers.Teacher;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
public class TeacherStudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeacherStudentsController> _logger;

    public TeacherStudentsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TeacherStudentsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Получить список студентов, которые проходили тесты преподавателя
    /// </summary>
    [HttpGet("students")]
    public async Task<ActionResult<object>> GetStudents(
        [FromQuery] int? quizId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "averageScore",
        [FromQuery] bool desc = true,
        [FromQuery] double? minScore = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var teacherQuizIds = await _context.Quizzes
            .Where(q => q.UserId == userId)
            .Select(q => q.Id)
            .ToListAsync();

        if (teacherQuizIds.Count == 0)
            return Ok(new { Message = "У вас ещё нет тестов", Students = new List<object>() });

        var query = _context.UserQuizAttempts
            .Where(qa => teacherQuizIds.Contains(qa.QuizId));

        if (quizId.HasValue)
            query = query.Where(qa => qa.QuizId == quizId.Value);

        var attempts = await query
            .Where(qa => qa.CompletedAt != null)
            .ToListAsync();

        var userIds = attempts.Select(a => a.UserId).Distinct().ToList();
        var quizIds = attempts.Select(a => a.QuizId).Distinct().ToList();
        
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);
        
        var quizzes = await _context.Quizzes
            .Where(q => quizIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        var allExamAttempts = await _context.UserExamAttempts
            .Where(ea => userIds.Contains(ea.UserId) && ea.CompletedAt != null)
            .ToListAsync();

        var examStatsDict = allExamAttempts
            .GroupBy(ea => ea.UserId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    ExamsTaken = g.Select(ea => ea.ExamId).Distinct().Count(),
                    LastExamDate = g.Max(ea => ea.CompletedAt)
                });

        var allFlashcardProgresses = await _context.UserFlashcardProgresses
            .Where(p => userIds.Contains(p.UserId) && p.LastReviewedAt != null)
            .ToListAsync();

        var flashcardStatsDict = allFlashcardProgresses
            .GroupBy(p => p.UserId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    CardsStudied = g.Count(),
                    LastCardDate = g.Max(p => (DateTime?)p.LastReviewedAt)
                });

        var studentStats = attempts
            .GroupBy(qa => qa.UserId)
            .Select(g =>
            {
                var lastQuizDate = g.Max(a => a.CompletedAt);
                var examStats = examStatsDict.TryGetValue(g.Key, out var es) 
                    ? es 
                    : new { ExamsTaken = 0, LastExamDate = (DateTime?)null };
                var flashcardStats = flashcardStatsDict.TryGetValue(g.Key, out var fs) 
                    ? fs 
                    : new { CardsStudied = 0, LastCardDate = (DateTime?)null };

                var lastActivityDate = new[] { lastQuizDate, examStats.LastExamDate, flashcardStats.LastCardDate }
                    .Where(d => d.HasValue)
                    .DefaultIfEmpty()
                    .Max();

                return new
                {
                    UserId = g.Key,
                    Email = users.TryGetValue(g.Key, out var user) ? user.Email : "Unknown",
                    UserName = users.TryGetValue(g.Key, out var u) ? u.UserName : "Unknown",
                    FirstName = users.TryGetValue(g.Key, out var usr) ? usr.FirstName : "",
                    LastName = users.TryGetValue(g.Key, out var us) ? us.LastName : "",
                    TotalAttempts = g.Count(),
                    AverageScore = Math.Round(g.Average(a => a.Percentage), 2),
                    BestScore = g.Max(a => a.Percentage),
                    LastAttemptDate = lastQuizDate,
                    LastActivityDate = lastActivityDate,
                    QuizzesTaken = g.Select(a => a.QuizId).Distinct().Count(),
                    ExamsTaken = examStats.ExamsTaken,
                    CardsStudied = flashcardStats.CardsStudied
                };
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            studentStats = studentStats
                .Where(s => 
                    s.Email.ToLower().Contains(searchLower) || 
                    s.UserName.ToLower().Contains(searchLower) ||
                    s.FirstName.ToLower().Contains(searchLower) ||
                    s.LastName.ToLower().Contains(searchLower))
                .ToList();
        }

        if (minScore.HasValue)
        {
            studentStats = studentStats.Where(s => s.AverageScore >= minScore.Value).ToList();
        }

        studentStats = sortBy?.ToLower() switch
        {
            "lastactivity" => desc 
                ? studentStats.OrderByDescending(s => s.LastActivityDate).ToList()
                : studentStats.OrderBy(s => s.LastActivityDate).ToList(),
            "attempts" => desc 
                ? studentStats.OrderByDescending(s => s.TotalAttempts).ToList()
                : studentStats.OrderBy(s => s.TotalAttempts).ToList(),
            "name" => desc 
                ? studentStats.OrderByDescending(s => s.LastName).ThenByDescending(s => s.FirstName).ToList()
                : studentStats.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList(),
            _ => desc
                ? studentStats.OrderByDescending(s => s.AverageScore).ToList()
                : studentStats.OrderBy(s => s.AverageScore).ToList()
        };

        var totalStudents = studentStats.Count;
        studentStats = studentStats
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            TotalStudents = totalStudents,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalStudents / (double)pageSize),
            Students = studentStats
        });
    }

    /// <summary>
    /// Получить детальную статистику по конкретному студенту
    /// </summary>
    [HttpGet("students/{studentId}/stats")]
    public async Task<ActionResult<object>> GetStudentStats(string studentId)
    {
        var userId = GetUserId();

        var student = await _userManager.FindByIdAsync(studentId);
        if (student == null)
            return NotFound(new { Message = "Студент не найден" });

        var teacherQuizIds = await _context.Quizzes
            .Where(q => q.UserId == userId)
            .Select(q => q.Id)
            .ToListAsync();

        var quizAttempts = await _context.UserQuizAttempts
            .Where(qa => qa.UserId == studentId && teacherQuizIds.Contains(qa.QuizId) && qa.CompletedAt != null)
            .OrderByDescending(qa => qa.CompletedAt)
            .ToListAsync();

        var examAttempts = await _context.UserExamAttempts
            .Where(ea => ea.UserId == studentId && ea.CompletedAt != null)
            .OrderByDescending(ea => ea.CompletedAt)
            .ToListAsync();

        var completedSetsCount = await _context.UserFlashcardSetAccesses
            .Where(a => a.UserId == studentId && a.IsCompleted)
            .CountAsync();
        
        var reviewedCardsCount = await _context.UserFlashcardProgresses
            .Where(p => p.UserId == studentId && p.LastReviewedAt != null)
            .CountAsync();
        
        var masteredCardsCount = await _context.UserFlashcardProgresses
            .Where(p => p.UserId == studentId && p.IsMastered)
            .CountAsync();

        var quizIds = quizAttempts.Select(a => a.QuizId).Distinct().ToList();
        var examIds = examAttempts.Select(ea => ea.ExamId).Distinct().ToList();
        
        var quizzesDict = await _context.Quizzes
            .Where(q => quizIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);
        
        var examsDict = await _context.Exams
            .Where(e => examIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        var flashcardSetsAccess = await _context.UserFlashcardSetAccesses
            .Where(a => a.UserId == studentId)
            .Include(a => a.FlashcardSet)
            .OrderByDescending(a => a.LastAccessedAt)
            .ToListAsync();

        var flashcardProgressBySet = await _context.UserFlashcardProgresses
            .Where(p => p.UserId == studentId)
            .Include(p => p.Flashcard)
            .GroupBy(p => p.Flashcard.FlashcardSetId)
            .Select(g => new
            {
                SetId = g.Key,
                ReviewedCards = g.Count(p => p.LastReviewedAt != null),
                MasteredCards = g.Count(p => p.IsMastered)
            })
            .ToDictionaryAsync(x => x.SetId);

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
            
            CompletedFlashcardSets = completedSetsCount,
            ReviewedCards = reviewedCardsCount,
            MasteredCards = masteredCardsCount,
            
            TotalQuizAttempts = quizAttempts.Count,
            QuizzesTaken = quizAttempts.Select(a => a.QuizId).Distinct().Count(),
            AverageQuizScore = quizAttempts.Any() ? Math.Round(quizAttempts.Average(a => a.Percentage), 2) : 0.0,
            BestQuizScore = quizAttempts.Any() ? quizAttempts.Max(a => a.Percentage) : 0.0,
            
            TotalExamAttempts = examAttempts.Count,
            ExamsTaken = examAttempts.Select(ea => ea.ExamId).Distinct().Count(),
            AverageExamScore = examAttempts.Any() ? Math.Round(examAttempts.Average(ea => ea.Percentage), 2) : 0.0,
            BestExamScore = examAttempts.Any() ? examAttempts.Max(ea => ea.Percentage) : 0.0,
            
            AverageScore = overallAverage,
            Attempts = attemptDetails,
            ExamAttempts = examAttemptDetails,
            
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
