using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Controllers.Teacher;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
public class TeacherExamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeacherExamsController> _logger;

    public TeacherExamsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TeacherExamsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Создать экзамен (для учителей)
    /// </summary>
    [HttpPost("exams")]
    public async Task<ActionResult<ExamDto>> CreateExam([FromBody] CreateExamDto dto)
    {
        if (dto.SubjectIds == null || !dto.SubjectIds.Any())
        {
            return BadRequest("Хотя бы один предмет должен быть выбран");
        }

        var userId = GetUserId();

        var exam = new Exam
        {
            Title = dto.Title,
            Description = dto.Description,
            Difficulty = dto.Difficulty,
            CountryId = dto.CountryId,
            UniversityId = dto.UniversityId,
            ExamTypeId = dto.ExamTypeId,
            MaxAttempts = dto.MaxAttempts,
            PassingScore = dto.PassingScore,
            IsProctored = dto.IsProctored,
            ShuffleQuestions = dto.ShuffleQuestions,
            ShuffleAnswers = dto.ShuffleAnswers,
            ShowResultsAfter = dto.ShowResultsAfter,
            ShowCorrectAnswers = dto.ShowCorrectAnswers,
            ShowDetailedFeedback = dto.ShowDetailedFeedback,
            TimeLimit = dto.TimeLimit,
            IsPublished = dto.IsPublished,
            IsPublic = dto.IsPublic,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            MaxScore = dto.Questions.Sum(q => q.Points)
        };

        foreach (var questionDto in dto.Questions)
        {
            var question = new ExamQuestion
            {
                Text = questionDto.Text,
                Explanation = questionDto.Explanation,
                Points = questionDto.Points,
                OrderIndex = questionDto.OrderIndex,
                Exam = exam
            };

            foreach (var answerDto in questionDto.Answers)
            {
                question.Answers.Add(new ExamAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    OrderIndex = answerDto.OrderIndex,
                    Question = question
                });
            }

            exam.Questions.Add(question);
        }

        // Добавляем предметы
        List<Subject>? subjects = null;
        if (dto.SubjectIds != null && dto.SubjectIds.Any())
        {
            subjects = await _context.Subjects
                .Where(s => dto.SubjectIds.Contains(s.Id))
                .ToListAsync();
            exam.Subjects = subjects;
        }

        if (dto.TagIds != null && dto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            exam.Tags = tags;
        }

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetExamById", "ExamsQuery", new { id = exam.Id }, new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Difficulty = exam.Difficulty,
            Subjects = subjects?.Select(s => new SubjectDto { Id = s.Id, Name = s.Name }).ToList() ?? new List<SubjectDto>(),
            SubjectIds = subjects?.Select(s => s.Id).ToList() ?? new List<int>(),
            MaxAttempts = exam.MaxAttempts,
            PassingScore = exam.PassingScore,
            IsPublished = exam.IsPublished,
            IsPublic = exam.IsPublic,
            MaxScore = exam.MaxScore,
            QuestionCount = exam.Questions.Count,
            CreatedAt = exam.CreatedAt,
            UserId = exam.UserId,
            Tags = exam.Tags.Select(t => t.Name).ToList()
        });
    }

    /// <summary>
    /// Получить свои экзамены
    /// </summary>
    [HttpGet("exams/my")]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetMyExams(
        [FromQuery] bool? isPublic = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        var query = _context.Exams
            .Where(e => e.UserId == userId);

        if (isPublic.HasValue)
            query = query.Where(e => e.IsPublic == isPublic.Value);

        var exams = await query
            .Include(e => e.Questions)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Difficulty = e.Difficulty,
                MaxAttempts = e.MaxAttempts,
                PassingScore = e.PassingScore,
                IsPublished = e.IsPublished,
                IsPublic = e.IsPublic,
                MaxScore = e.MaxScore,
                QuestionCount = e.Questions.Count,
                CreatedAt = e.CreatedAt,
                UserId = e.UserId
            })
            .ToListAsync();

        return Ok(exams);
    }

    /// <summary>
    /// Получить статистику по экзамену (только свои экзамены)
    /// </summary>
    [HttpGet("exams/{examId}/stats")]
    public async Task<ActionResult> GetExamStats(int examId)
    {
        var userId = GetUserId();

        var exam = await _context.Exams
            .Include(e => e.Questions)
            .Include(e => e.Attempts)
            .FirstOrDefaultAsync(e => e.Id == examId && e.UserId == userId);

        if (exam == null)
            return NotFound("Экзамен не найден или доступ запрещен");

        var completedAttempts = exam.Attempts.Where(a => a.CompletedAt != null).ToList();
        var totalAttempts = completedAttempts.Count;
        var averageScore = totalAttempts > 0 ? completedAttempts.Average(a => a.Percentage) : 0;
        var passedCount = completedAttempts.Count(a => a.Percentage >= exam.PassingScore);
        var passRate = totalAttempts > 0 ? (double)passedCount / totalAttempts * 100 : 0;

        var uniqueStudents = completedAttempts
            .Select(a => a.UserId)
            .Distinct()
            .Count();

        return Ok(new
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            TotalQuestions = exam.Questions.Count,
            MaxScore = exam.MaxScore,
            PassingScore = exam.PassingScore,
            TotalAttempts = totalAttempts,
            UniqueStudents = uniqueStudents,
            AverageScore = Math.Round(averageScore, 2),
            PassedCount = passedCount,
            PassRate = Math.Round(passRate, 2),
            BestScore = totalAttempts > 0 ? completedAttempts.Max(a => a.Percentage) : 0,
            WorstScore = totalAttempts > 0 ? completedAttempts.Min(a => a.Percentage) : 0
        });
    }

    /// <summary>
    /// Переключить публичность экзамена
    /// </summary>
    [HttpPatch("exams/{id}/toggle-public")]
    public async Task<IActionResult> TogglePublic(int id)
    {
        var userId = GetUserId();

        var exam = await _context.Exams
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (exam == null)
            return NotFound("Экзамен не найден");

        exam.IsPublic = !exam.IsPublic;
        exam.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { IsPublic = exam.IsPublic });
    }

    /// <summary>
    /// Получить общую статистику по всем экзаменам учителя
    /// </summary>
    [HttpGet("exams/stats/overview")]
    public async Task<ActionResult> GetOverviewStats()
    {
        var userId = GetUserId();

        var exams = await _context.Exams
            .Where(e => e.UserId == userId)
            .Include(e => e.Questions)
            .ToListAsync();

        var examIds = exams.Select(e => e.Id).ToList();

        var allAttempts = await _context.UserExamAttempts
            .Where(a => examIds.Contains(a.ExamId) && a.CompletedAt != null)
            .ToListAsync();

        var totalExams = exams.Count;
        var publishedExams = exams.Count(e => e.IsPublished);
        var publicExams = exams.Count(e => e.IsPublic);
        var totalAttempts = allAttempts.Count;
        var uniqueStudents = allAttempts.Select(a => a.UserId).Distinct().Count();
        var averageScore = totalAttempts > 0 ? allAttempts.Average(a => a.Percentage) : 0;

        return Ok(new
        {
            TotalExams = totalExams,
            PublishedExams = publishedExams,
            PublicExams = publicExams,
            TotalAttempts = totalAttempts,
            UniqueStudents = uniqueStudents,
            AverageScore = Math.Round(averageScore, 2)
        });
    }

    /// <summary>
    /// Экспортировать результаты экзамена в CSV
    /// </summary>
    [HttpGet("exams/{examId}/export-results")]
    public async Task<IActionResult> ExportExamResults(int examId)
    {
        var userId = GetUserId();

        var exam = await _context.Exams
            .Include(e => e.Attempts)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(e => e.Id == examId && e.UserId == userId);

        if (exam == null)
            return NotFound("Экзамен не найден");

        var attempts = exam.Attempts
            .Where(a => a.CompletedAt != null)
            .OrderByDescending(a => a.CompletedAt)
            .ToList();

        var csv = new StringBuilder();
        csv.AppendLine("Student Name,Email,Score,Total Points,Percentage,Passed,Completed At,Time Spent");

        foreach (var attempt in attempts)
        {
            var passed = attempt.Percentage >= exam.PassingScore ? "Yes" : "No";
            csv.AppendLine($"{attempt.User.FirstName} {attempt.User.LastName},{attempt.User.Email},{attempt.Score},{attempt.MaxScore},{attempt.Percentage:F2},{passed},{attempt.CompletedAt:yyyy-MM-dd HH:mm:ss},{attempt.TimeSpentSeconds}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"exam_{examId}_results_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
