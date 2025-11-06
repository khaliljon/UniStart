using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<string> GetUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? throw new UnauthorizedAccessException("Пользователь не авторизован");
    }

    /// <summary>
    /// Получить все опубликованные тесты (для студентов)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TestDto>>> GetAllTests(
        [FromQuery] string? subject = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Tests
            .Include(t => t.User)
            .Include(t => t.Tags)
            .Include(t => t.Questions)
            .Where(t => t.IsPublished);

        if (!string.IsNullOrEmpty(subject))
            query = query.Where(t => t.Subject == subject);

        if (!string.IsNullOrEmpty(difficulty))
            query = query.Where(t => t.Difficulty == difficulty);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.Title.Contains(search) || 
                                    (t.Description != null && t.Description.Contains(search)));

        var tests = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TestDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Subject = t.Subject,
                Difficulty = t.Difficulty,
                MaxAttempts = t.MaxAttempts,
                PassingScore = t.PassingScore,
                IsProctored = t.IsProctored,
                ShuffleQuestions = t.ShuffleQuestions,
                ShuffleAnswers = t.ShuffleAnswers,
                ShowResultsAfter = t.ShowResultsAfter,
                ShowCorrectAnswers = t.ShowCorrectAnswers,
                ShowDetailedFeedback = t.ShowDetailedFeedback,
                TimeLimit = t.TimeLimit,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                IsPublished = t.IsPublished,
                TotalPoints = t.TotalPoints,
                QuestionCount = t.Questions.Count,
                CreatedAt = t.CreatedAt,
                UserId = t.UserId,
                UserName = t.User.UserName ?? "Unknown",
                Tags = t.Tags.Select(tag => tag.Name).ToList()
            })
            .ToListAsync();

        return Ok(tests);
    }

    /// <summary>
    /// Получить свои тесты (для преподавателей и админов)
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<List<TestDto>>> GetMyTests()
    {
        var userId = await GetUserId();

        var tests = await _context.Tests
            .Include(t => t.User)
            .Include(t => t.Tags)
            .Include(t => t.Questions)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TestDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Subject = t.Subject,
                Difficulty = t.Difficulty,
                MaxAttempts = t.MaxAttempts,
                PassingScore = t.PassingScore,
                IsProctored = t.IsProctored,
                ShuffleQuestions = t.ShuffleQuestions,
                ShuffleAnswers = t.ShuffleAnswers,
                ShowResultsAfter = t.ShowResultsAfter,
                ShowCorrectAnswers = t.ShowCorrectAnswers,
                ShowDetailedFeedback = t.ShowDetailedFeedback,
                TimeLimit = t.TimeLimit,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                IsPublished = t.IsPublished,
                TotalPoints = t.TotalPoints,
                QuestionCount = t.Questions.Count,
                CreatedAt = t.CreatedAt,
                UserId = t.UserId,
                UserName = t.User.UserName ?? "Unknown",
                Tags = t.Tags.Select(tag => tag.Name).ToList()
            })
            .ToListAsync();

        return Ok(tests);
    }

    /// <summary>
    /// Получить тест по ID (детали)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TestDto>> GetTestById(int id)
    {
        var test = await _context.Tests
            .Include(t => t.User)
            .Include(t => t.Tags)
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (test == null)
            return NotFound("Тест не найден");

        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        // Проверка доступа: должен быть опубликован или создан текущим пользователем или админом
        if (!test.IsPublished && test.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        var testDto = new TestDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            Subject = test.Subject,
            Difficulty = test.Difficulty,
            MaxAttempts = test.MaxAttempts,
            PassingScore = test.PassingScore,
            IsProctored = test.IsProctored,
            ShuffleQuestions = test.ShuffleQuestions,
            ShuffleAnswers = test.ShuffleAnswers,
            ShowResultsAfter = test.ShowResultsAfter,
            ShowCorrectAnswers = test.ShowCorrectAnswers,
            ShowDetailedFeedback = test.ShowDetailedFeedback,
            TimeLimit = test.TimeLimit,
            StartDate = test.StartDate,
            EndDate = test.EndDate,
            IsPublished = test.IsPublished,
            TotalPoints = test.TotalPoints,
            QuestionCount = test.Questions.Count,
            CreatedAt = test.CreatedAt,
            UserId = test.UserId,
            UserName = test.User.UserName ?? "Unknown",
            Tags = test.Tags.Select(tag => tag.Name).ToList()
        };

        return Ok(testDto);
    }

    /// <summary>
    /// Получить тест для прохождения (без правильных ответов)
    /// </summary>
    [HttpGet("{id}/take")]
    public async Task<ActionResult<TestTakingDto>> GetTestForTaking(int id)
    {
        var userId = await GetUserId();

        var test = await _context.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Attempts.Where(a => a.UserId == userId))
            .FirstOrDefaultAsync(t => t.Id == id && t.IsPublished);

        if (test == null)
            return NotFound("Тест не найден или не опубликован");

        // Проверка временных ограничений
        var now = DateTime.UtcNow;
        if (test.StartDate.HasValue && now < test.StartDate.Value)
            return BadRequest("Тест ещё не начался");

        if (test.EndDate.HasValue && now > test.EndDate.Value)
            return BadRequest("Дедлайн теста истёк");

        // Проверка количества попыток
        var attemptsCount = test.Attempts.Count(a => a.CompletedAt != null);
        if (attemptsCount >= test.MaxAttempts)
            return BadRequest($"Вы использовали все попытки ({test.MaxAttempts})");

        var questions = test.Questions.OrderBy(q => q.Order).ToList();

        // Перемешиваем вопросы если нужно
        if (test.ShuffleQuestions)
            questions = questions.OrderBy(_ => Guid.NewGuid()).ToList();

        var testDto = new TestTakingDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            Subject = test.Subject,
            TimeLimit = test.TimeLimit,
            TotalPoints = test.TotalPoints,
            MaxAttempts = test.MaxAttempts,
            RemainingAttempts = test.MaxAttempts - attemptsCount,
            ShuffleQuestions = test.ShuffleQuestions,
            ShuffleAnswers = test.ShuffleAnswers,
            Questions = questions.Select(q => new TestQuestionTakingDto
            {
                Id = q.Id,
                Text = q.Text,
                Points = q.Points,
                Order = q.Order,
                Answers = (test.ShuffleAnswers 
                    ? q.Answers.OrderBy(_ => Guid.NewGuid()) 
                    : q.Answers.OrderBy(a => a.Order))
                    .Select(a => new TestAnswerTakingDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        Order = a.Order
                    }).ToList()
            }).ToList()
        };

        return Ok(testDto);
    }

    /// <summary>
    /// Отправить ответы на тест
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<TestResultDto>> SubmitTest([FromBody] SubmitTestDto submission)
    {
        var userId = await GetUserId();

        var test = await _context.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Attempts.Where(a => a.UserId == userId))
            .FirstOrDefaultAsync(t => t.Id == submission.TestId);

        if (test == null)
            return NotFound("Тест не найден");

        // Проверка количества попыток
        var completedAttempts = test.Attempts.Count(a => a.CompletedAt != null);
        if (completedAttempts >= test.MaxAttempts)
            return BadRequest("Вы использовали все попытки");

        // Создаём попытку
        var attempt = new UserTestAttempt
        {
            UserId = userId,
            TestId = test.Id,
            StartedAt = DateTime.UtcNow.Subtract(submission.TimeSpent),
            CompletedAt = DateTime.UtcNow,
            TimeSpent = submission.TimeSpent,
            AttemptNumber = completedAttempts + 1,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };

        // Проверяем ответы и подсчитываем баллы
        int totalScore = 0;
        int totalPoints = test.TotalPoints;
        var userAnswers = new List<UserTestAnswer>();

        foreach (var answer in submission.Answers)
        {
            var question = test.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == answer.SelectedAnswerId);
            if (selectedAnswer == null) continue;

            var isCorrect = selectedAnswer.IsCorrect;
            var pointsEarned = isCorrect ? question.Points : 0;
            totalScore += pointsEarned;

            userAnswers.Add(new UserTestAnswer
            {
                Attempt = attempt,
                QuestionId = answer.QuestionId,
                SelectedAnswerId = answer.SelectedAnswerId,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                AnsweredAt = DateTime.UtcNow
            });
        }

        attempt.Score = totalScore;
        attempt.TotalPoints = totalPoints;
        attempt.Percentage = totalPoints > 0 ? (double)totalScore / totalPoints * 100 : 0;
        attempt.Passed = attempt.Percentage >= test.PassingScore;
        attempt.UserAnswers = userAnswers;

        _context.UserTestAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Формируем результат
        var result = new TestResultDto
        {
            AttemptId = attempt.Id,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            Percentage = attempt.Percentage,
            Passed = attempt.Passed,
            TimeSpent = attempt.TimeSpent,
            CompletedAt = attempt.CompletedAt.Value,
            AttemptNumber = attempt.AttemptNumber,
            RemainingAttempts = test.MaxAttempts - attempt.AttemptNumber,
            ShowCorrectAnswers = test.ShowCorrectAnswers,
            ShowDetailedFeedback = test.ShowDetailedFeedback
        };

        // Добавляем детальные результаты если разрешено показывать сразу
        if (test.ShowResultsAfter == "Immediate")
        {
            result.QuestionResults = test.Questions.Select(q =>
            {
                var userAnswer = userAnswers.FirstOrDefault(ua => ua.QuestionId == q.Id);
                var correctAnswer = q.Answers.FirstOrDefault(a => a.IsCorrect);

                return new TestQuestionResultDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    Points = q.Points,
                    PointsEarned = userAnswer?.PointsEarned ?? 0,
                    IsCorrect = userAnswer?.IsCorrect ?? false,
                    Explanation = test.ShowDetailedFeedback ? q.Explanation : null,
                    SelectedAnswerId = userAnswer?.SelectedAnswerId ?? 0,
                    SelectedAnswerText = userAnswer != null 
                        ? q.Answers.FirstOrDefault(a => a.Id == userAnswer.SelectedAnswerId)?.Text ?? ""
                        : "",
                    CorrectAnswerId = test.ShowCorrectAnswers ? correctAnswer?.Id : null,
                    CorrectAnswerText = test.ShowCorrectAnswers ? correctAnswer?.Text : null
                };
            }).ToList();
        }

        return Ok(result);
    }

    /// <summary>
    /// Получить статистику теста (для преподавателей и админов)
    /// </summary>
    [HttpGet("{id}/stats")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<TestStatsDto>> GetTestStats(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var test = await _context.Tests
            .Include(t => t.Questions)
            .Include(t => t.Attempts)
                .ThenInclude(a => a.User)
            .Include(t => t.Attempts)
                .ThenInclude(a => a.UserAnswers)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (test == null)
            return NotFound("Тест не найден");

        // Проверка доступа
        if (test.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        var completedAttempts = test.Attempts.Where(a => a.CompletedAt != null).ToList();

        var stats = new TestStatsDto
        {
            TestId = test.Id,
            Title = test.Title,
            TotalAttempts = completedAttempts.Count,
            UniqueStudents = completedAttempts.Select(a => a.UserId).Distinct().Count(),
            AverageScore = completedAttempts.Any() ? completedAttempts.Average(a => a.Percentage) : 0,
            PassRate = completedAttempts.Any() 
                ? (double)completedAttempts.Count(a => a.Passed) / completedAttempts.Count * 100 
                : 0,
            AverageTimeSpent = completedAttempts.Any() 
                ? TimeSpan.FromSeconds(completedAttempts.Average(a => a.TimeSpent.TotalSeconds))
                : TimeSpan.Zero,
            QuestionStats = test.Questions.Select(q =>
            {
                var allAnswers = completedAttempts
                    .SelectMany(a => a.UserAnswers)
                    .Where(ua => ua.QuestionId == q.Id)
                    .ToList();

                return new TestQuestionStatsDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    TotalAnswers = allAnswers.Count,
                    CorrectAnswers = allAnswers.Count(ua => ua.IsCorrect),
                    SuccessRate = allAnswers.Any() 
                        ? (double)allAnswers.Count(ua => ua.IsCorrect) / allAnswers.Count * 100 
                        : 0
                };
            }).ToList(),
            RecentAttempts = completedAttempts
                .OrderByDescending(a => a.CompletedAt)
                .Take(10)
                .Select(a => new TestAttemptSummaryDto
                {
                    AttemptId = a.Id,
                    StudentName = a.User.UserName ?? "Unknown",
                    Score = a.Score,
                    TotalPoints = a.TotalPoints,
                    Percentage = a.Percentage,
                    Passed = a.Passed,
                    TimeSpent = a.TimeSpent,
                    CompletedAt = a.CompletedAt.Value
                })
                .ToList()
        };

        return Ok(stats);
    }

    /// <summary>
    /// Создать новый тест
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<TestDto>> CreateTest([FromBody] CreateTestDto dto)
    {
        var userId = await GetUserId();

        var test = new Test
        {
            Title = dto.Title,
            Description = dto.Description,
            Subject = dto.Subject,
            Difficulty = dto.Difficulty,
            MaxAttempts = dto.MaxAttempts,
            PassingScore = dto.PassingScore,
            IsProctored = dto.IsProctored,
            ShuffleQuestions = dto.ShuffleQuestions,
            ShuffleAnswers = dto.ShuffleAnswers,
            ShowResultsAfter = dto.ShowResultsAfter,
            ShowCorrectAnswers = dto.ShowCorrectAnswers,
            ShowDetailedFeedback = dto.ShowDetailedFeedback,
            TimeLimit = dto.TimeLimit,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            TotalPoints = dto.Questions.Sum(q => q.Points)
        };

        // Добавляем вопросы
        foreach (var questionDto in dto.Questions)
        {
            var question = new TestQuestion
            {
                Text = questionDto.Text,
                Explanation = questionDto.Explanation,
                Points = questionDto.Points,
                Order = questionDto.Order,
                Test = test
            };

            foreach (var answerDto in questionDto.Answers)
            {
                var answer = new TestAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    Order = answerDto.Order,
                    Question = question
                };
                question.Answers.Add(answer);
            }

            test.Questions.Add(question);
        }

        // Добавляем теги
        if (dto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            test.Tags = tags;
        }

        _context.Tests.Add(test);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTestById), new { id = test.Id }, new TestDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            Subject = test.Subject,
            Difficulty = test.Difficulty,
            MaxAttempts = test.MaxAttempts,
            PassingScore = test.PassingScore,
            IsPublished = test.IsPublished,
            TotalPoints = test.TotalPoints,
            QuestionCount = test.Questions.Count,
            CreatedAt = test.CreatedAt,
            UserId = test.UserId,
            Tags = test.Tags.Select(t => t.Name).ToList()
        });
    }

    /// <summary>
    /// Обновить тест
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult> UpdateTest(int id, [FromBody] CreateTestDto dto)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var test = await _context.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (test == null)
            return NotFound("Тест не найден");

        // Проверка доступа
        if (test.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        // Обновляем поля
        test.Title = dto.Title;
        test.Description = dto.Description;
        test.Subject = dto.Subject;
        test.Difficulty = dto.Difficulty;
        test.MaxAttempts = dto.MaxAttempts;
        test.PassingScore = dto.PassingScore;
        test.IsProctored = dto.IsProctored;
        test.ShuffleQuestions = dto.ShuffleQuestions;
        test.ShuffleAnswers = dto.ShuffleAnswers;
        test.ShowResultsAfter = dto.ShowResultsAfter;
        test.ShowCorrectAnswers = dto.ShowCorrectAnswers;
        test.ShowDetailedFeedback = dto.ShowDetailedFeedback;
        test.TimeLimit = dto.TimeLimit;
        test.StartDate = dto.StartDate;
        test.EndDate = dto.EndDate;
        test.UpdatedAt = DateTime.UtcNow;
        test.TotalPoints = dto.Questions.Sum(q => q.Points);

        // Удаляем старые вопросы
        _context.TestQuestions.RemoveRange(test.Questions);

        // Добавляем новые вопросы
        test.Questions.Clear();
        foreach (var questionDto in dto.Questions)
        {
            var question = new TestQuestion
            {
                Text = questionDto.Text,
                Explanation = questionDto.Explanation,
                Points = questionDto.Points,
                Order = questionDto.Order,
                Test = test
            };

            foreach (var answerDto in questionDto.Answers)
            {
                var answer = new TestAnswer
                {
                    Text = answerDto.Text,
                    IsCorrect = answerDto.IsCorrect,
                    Order = answerDto.Order,
                    Question = question
                };
                question.Answers.Add(answer);
            }

            test.Questions.Add(question);
        }

        // Обновляем теги
        test.Tags.Clear();
        if (dto.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            test.Tags = tags;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Опубликовать/снять с публикации тест
    /// </summary>
    [HttpPatch("{id}/publish")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult> TogglePublish(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var test = await _context.Tests.FindAsync(id);
        if (test == null)
            return NotFound("Тест не найден");

        // Проверка доступа
        if (test.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        test.IsPublished = !test.IsPublished;
        test.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { isPublished = test.IsPublished });
    }

    /// <summary>
    /// Удалить тест
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult> DeleteTest(int id)
    {
        var userId = await GetUserId();
        var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

        var test = await _context.Tests.FindAsync(id);
        if (test == null)
            return NotFound("Тест не найден");

        // Проверка доступа
        if (test.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        _context.Tests.Remove(test);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
