using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;
using System.Text.Json;

namespace UniStart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuizzesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============ QUIZ CRUD ============

        /// <summary>
        /// Получить все тесты (список)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<QuizDto>>> GetQuizzes(
            [FromQuery] string? subject = null,
            [FromQuery] string? difficulty = null)
        {
            var query = _context.Quizzes
                .Include(q => q.Questions)
                .Where(q => q.IsPublished);

            if (!string.IsNullOrEmpty(subject))
                query = query.Where(q => q.Subject == subject);

            if (!string.IsNullOrEmpty(difficulty))
                query = query.Where(q => q.Difficulty == difficulty);

            var quizzes = await query
                .Select(q => new QuizDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    TimeLimit = q.TimeLimit,
                    Subject = q.Subject,
                    Difficulty = q.Difficulty,
                    QuestionCount = q.Questions.Count,
                    TotalPoints = q.Questions.Sum(qu => qu.Points)
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        /// <summary>
        /// Получить тест по ID с вопросами (БЕЗ правильных ответов для прохождения)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDetailDto>> GetQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound($"Quiz with ID {id} not found");

            var quizDto = new QuizDetailDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimit = quiz.TimeLimit,
                Subject = quiz.Subject,
                Difficulty = quiz.Difficulty,
                Questions = quiz.Questions.OrderBy(q => q.OrderIndex).Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    Points = q.Points,
                    ImageUrl = q.ImageUrl,
                    Answers = q.Answers.OrderBy(a => a.OrderIndex).Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = null // Скрываем правильные ответы при прохождении
                    }).ToList()
                }).ToList()
            };

            return Ok(quizDto);
        }

        /// <summary>
        /// Создать новый тест
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Quiz>> CreateQuiz(CreateQuizDto dto)
        {
            var quiz = new Quiz
            {
                Title = dto.Title,
                Description = dto.Description,
                TimeLimit = dto.TimeLimit,
                Subject = dto.Subject,
                Difficulty = dto.Difficulty
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
        }

        /// <summary>
        /// Обновить тест
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, CreateQuizDto dto)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound();

            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.TimeLimit = dto.TimeLimit;
            quiz.Subject = dto.Subject;
            quiz.Difficulty = dto.Difficulty;
            quiz.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить тест
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Опубликовать/снять с публикации тест
        /// </summary>
        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> TogglePublish(int id, [FromBody] bool isPublished)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound();

            quiz.IsPublished = isPublished;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============ SUBMIT & GRADE QUIZ ============

        /// <summary>
        /// Отправить ответы на тест и получить результаты
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<QuizResultDto>> SubmitQuiz(SubmitQuizDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Answers)
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

            if (quiz == null)
                return NotFound("Quiz not found");

            // Проверка ответов и подсчет результатов
            var questionResults = new List<QuestionResultDto>();
            int totalScore = 0;
            int maxScore = quiz.Questions.Sum(q => q.Points);

            foreach (var question in quiz.Questions)
            {
                var correctAnswerIds = question.Answers
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .OrderBy(id => id)
                    .ToList();

                var userAnswerIds = dto.UserAnswers.ContainsKey(question.Id)
                    ? dto.UserAnswers[question.Id].OrderBy(id => id).ToList()
                    : new List<int>();

                bool isCorrect = correctAnswerIds.SequenceEqual(userAnswerIds);
                int pointsEarned = isCorrect ? question.Points : 0;
                totalScore += pointsEarned;

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned,
                    MaxPoints = question.Points,
                    CorrectAnswerIds = correctAnswerIds,
                    UserAnswerIds = userAnswerIds,
                    Explanation = question.Explanation
                });
            }

            // Сохраняем попытку пользователя
            var attempt = new UserQuizAttempt
            {
                UserId = "anonymous", // TODO: заменить на реального пользователя после авторизации
                QuizId = dto.QuizId,
                Score = totalScore,
                MaxScore = maxScore,
                Percentage = maxScore > 0 ? (double)totalScore / maxScore * 100 : 0,
                TimeSpentSeconds = dto.TimeSpentSeconds,
                CompletedAt = DateTime.UtcNow,
                UserAnswersJson = JsonSerializer.Serialize(dto.UserAnswers)
            };

            _context.UserQuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            var result = new QuizResultDto
            {
                Score = totalScore,
                MaxScore = maxScore,
                Percentage = attempt.Percentage,
                TimeSpentSeconds = dto.TimeSpentSeconds,
                QuestionResults = questionResults
            };

            return Ok(result);
        }

        /// <summary>
        /// Получить историю попыток пользователя по тесту
        /// </summary>
        [HttpGet("{quizId}/attempts")]
        public async Task<ActionResult<List<UserQuizAttempt>>> GetQuizAttempts(int quizId)
        {
            var attempts = await _context.UserQuizAttempts
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.CompletedAt)
                .Take(10)
                .ToListAsync();

            return Ok(attempts);
        }

        /// <summary>
        /// Получить статистику по тесту
        /// </summary>
        [HttpGet("{quizId}/statistics")]
        public async Task<ActionResult> GetQuizStatistics(int quizId)
        {
            var attempts = await _context.UserQuizAttempts
                .Where(a => a.QuizId == quizId && a.CompletedAt != null)
                .ToListAsync();

            if (!attempts.Any())
                return Ok(new { message = "No attempts found" });

            var stats = new
            {
                TotalAttempts = attempts.Count,
                AverageScore = attempts.Average(a => a.Percentage),
                HighestScore = attempts.Max(a => a.Percentage),
                LowestScore = attempts.Min(a => a.Percentage),
                AverageTime = attempts.Average(a => a.TimeSpentSeconds)
            };

            return Ok(stats);
        }
    }
}
