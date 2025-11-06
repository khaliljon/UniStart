using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models;

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

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ============ QUIZ CRUD ============

        /// <summary>
        /// Получить все опубликованные тесты (публичный доступ) с поиском и фильтрацией
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<QuizDto>>> GetQuizzes(
            [FromQuery] string? search = null,
            [FromQuery] string? subject = null,
            [FromQuery] string? difficulty = null,
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null)
        {
            var query = _context.Quizzes
                .Include(q => q.Questions)
                .Where(q => q.IsPublished);

            // Поиск по названию и описанию
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => 
                    q.Title.ToLower().Contains(search.ToLower()) ||
                    q.Description.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(subject))
                query = query.Where(q => q.Subject == subject);

            if (!string.IsNullOrEmpty(difficulty))
                query = query.Where(q => q.Difficulty == difficulty);

            // Пагинация
            if (page.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

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
        /// Получить только свои тесты (все, включая неопубликованные) с поиском и фильтрацией
        /// </summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<List<QuizDto>>> GetMyQuizzes(
            [FromQuery] string? search = null,
            [FromQuery] string? subject = null,
            [FromQuery] string? difficulty = null,
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null)
        {
            var userId = GetUserId()!;
            
            var query = _context.Quizzes
                .Where(q => q.UserId == userId);

            // Поиск по названию и описанию
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => 
                    q.Title.ToLower().Contains(search.ToLower()) ||
                    q.Description.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(subject))
                query = query.Where(q => q.Subject == subject);

            if (!string.IsNullOrEmpty(difficulty))
                query = query.Where(q => q.Difficulty == difficulty);

            // Пагинация
            if (page.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var quizzes = await query
                .Include(q => q.Questions)
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
        [Authorize]
        public async Task<ActionResult<Quiz>> CreateQuiz(CreateQuizDto dto)
        {
            var userId = GetUserId()!;
            
            var quiz = new Quiz
            {
                Title = dto.Title,
                Description = dto.Description,
                TimeLimit = dto.TimeLimit,
                Subject = dto.Subject,
                Difficulty = dto.Difficulty,
                UserId = userId
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
        }

        /// <summary>
        /// Обновить тест (только свои)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz(int id, CreateQuizDto dto)
        {
            var userId = GetUserId()!;
            
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);
                
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
        /// Удалить тест (только свои)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var userId = GetUserId()!;
            
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);
                
            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Опубликовать/снять с публикации тест (только свои)
        /// </summary>
        [HttpPatch("{id}/publish")]
        [Authorize]
        public async Task<IActionResult> TogglePublish(int id, [FromBody] bool isPublished)
        {
            var userId = GetUserId()!;
            
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);
                
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
        [Authorize]
        public async Task<ActionResult<QuizResultDto>> SubmitQuiz(SubmitQuizDto dto)
        {
            var userId = GetUserId()!;
            
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
                UserId = userId,
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
        /// Получить историю попыток текущего пользователя по тесту
        /// </summary>
        [HttpGet("{quizId}/attempts")]
        [Authorize]
        public async Task<ActionResult<List<UserQuizAttempt>>> GetQuizAttempts(int quizId)
        {
            var userId = GetUserId()!;
            
            var attempts = await _context.UserQuizAttempts
                .Where(a => a.QuizId == quizId && a.UserId == userId)
                .OrderByDescending(a => a.CompletedAt)
                .Take(10)
                .ToListAsync();

            return Ok(attempts);
        }

        /// <summary>
        /// Получить подробную статистику по тесту для страницы статистики (только для владельца теста)
        /// </summary>
        [HttpGet("{id}/stats")]
        [Authorize]
        public async Task<ActionResult> GetQuizStats(int id)
        {
            var userId = GetUserId()!;
            
            // Проверяем, что тест принадлежит текущему пользователю или пользователь админ
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
                
            if (quiz == null)
                return NotFound("Quiz not found");

            // Проверка доступа: владелец или админ
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (quiz.UserId != userId && !userRoles.Contains("Admin") && !userRoles.Contains("Teacher"))
                return Forbid();
            
            var attempts = await _context.UserQuizAttempts
                .Include(a => a.User)
                .Where(a => a.QuizId == id && a.CompletedAt != null)
                .OrderByDescending(a => a.CompletedAt)
                .ToListAsync();

            // Если нет попыток, возвращаем пустую статистику
            if (!attempts.Any())
            {
                return Ok(new
                {
                    quizId = id,
                    quizTitle = quiz.Title,
                    totalAttempts = 0,
                    averageScore = 0.0,
                    averageTimeSpent = 0,
                    passRate = 0.0,
                    questionStats = new List<object>(),
                    recentAttempts = new List<object>()
                });
            }

            // Подсчет статистики по вопросам
            var questionStats = new List<object>();
            foreach (var question in quiz.Questions.OrderBy(q => q.OrderIndex))
            {
                int totalAnswers = 0;
                int correctAnswers = 0;

                foreach (var attempt in attempts)
                {
                    if (string.IsNullOrEmpty(attempt.UserAnswersJson))
                        continue;

                    try
                    {
                        var userAnswers = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(attempt.UserAnswersJson);
                        if (userAnswers == null || !userAnswers.ContainsKey(question.Id))
                            continue;

                        totalAnswers++;

                        var correctAnswerIds = question.Answers
                            .Where(a => a.IsCorrect)
                            .Select(a => a.Id)
                            .OrderBy(id => id)
                            .ToList();

                        var userAnswerIds = userAnswers[question.Id].OrderBy(id => id).ToList();

                        if (correctAnswerIds.SequenceEqual(userAnswerIds))
                            correctAnswers++;
                    }
                    catch
                    {
                        // Пропускаем невалидные данные
                    }
                }

                questionStats.Add(new
                {
                    questionId = question.Id,
                    questionText = question.Text,
                    correctAnswers,
                    totalAnswers,
                    successRate = totalAnswers > 0 ? (double)correctAnswers / totalAnswers * 100 : 0
                });
            }

            // Последние попытки
            var recentAttempts = attempts.Take(10).Select(a => new
            {
                id = a.Id,
                studentName = $"{a.User?.FirstName} {a.User?.LastName}".Trim(),
                score = a.Score,
                maxScore = a.MaxScore,
                percentage = a.Percentage,
                timeSpent = a.TimeSpentSeconds,
                completedAt = a.CompletedAt
            }).ToList();

            // Общая статистика
            var stats = new
            {
                quizId = id,
                quizTitle = quiz.Title,
                totalAttempts = attempts.Count,
                averageScore = attempts.Average(a => a.Percentage),
                averageTimeSpent = (int)attempts.Average(a => a.TimeSpentSeconds),
                passRate = attempts.Count(a => a.Percentage >= 50) * 100.0 / attempts.Count,
                questionStats,
                recentAttempts
            };

            return Ok(stats);
        }

        /// <summary>
        /// Получить статистику по тесту (только для владельца теста)
        /// </summary>
        [HttpGet("{quizId}/statistics")]
        [Authorize]
        public async Task<ActionResult> GetQuizStatistics(int quizId)
        {
            var userId = GetUserId()!;
            
            // Проверяем, что тест принадлежит текущему пользователю
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId && q.UserId == userId);
                
            if (quiz == null)
                return NotFound("Quiz not found or access denied");
            
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

        // ============ QUESTIONS CRUD ============

        /// <summary>
        /// Создать новый вопрос для квиза (только для владельца квиза)
        /// </summary>
        [HttpPost("questions")]
        [Authorize]
        public async Task<ActionResult<Question>> CreateQuestion(CreateQuestionDto dto)
        {
            var userId = GetUserId()!;
            
            // Проверяем, что квиз принадлежит текущему пользователю
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId && q.UserId == userId);
                
            if (quiz == null)
                return NotFound("Quiz not found or access denied");

            var question = new Question
            {
                Text = dto.Text,
                QuestionType = dto.QuestionType,
                Points = dto.Points,
                ImageUrl = dto.ImageUrl,
                Explanation = dto.Explanation,
                QuizId = dto.QuizId,
                OrderIndex = await _context.Questions
                    .Where(q => q.QuizId == dto.QuizId)
                    .CountAsync()
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
        }

        /// <summary>
        /// Получить вопрос по ID
        /// </summary>
        [HttpGet("questions/{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            return Ok(question);
        }

        /// <summary>
        /// Обновить вопрос (только для владельца квиза)
        /// </summary>
        [HttpPut("questions/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestion(int id, UpdateQuestionDto dto)
        {
            var userId = GetUserId()!;
            
            var question = await _context.Questions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == userId);
                
            if (question == null)
                return NotFound("Question not found or access denied");

            question.Text = dto.Text;
            question.QuestionType = dto.QuestionType;
            question.Points = dto.Points;
            question.ImageUrl = dto.ImageUrl;
            question.Explanation = dto.Explanation;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить вопрос (только для владельца квиза)
        /// </summary>
        [HttpDelete("questions/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var userId = GetUserId()!;
            
            var question = await _context.Questions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == userId);
                
            if (question == null)
                return NotFound("Question not found or access denied");

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============ ANSWERS CRUD ============

        /// <summary>
        /// Создать новый ответ для вопроса (только для владельца квиза)
        /// </summary>
        [HttpPost("answers")]
        [Authorize]
        public async Task<ActionResult<Answer>> CreateAnswer(CreateAnswerDto dto)
        {
            var userId = GetUserId()!;
            
            // Проверяем, что вопрос принадлежит квизу пользователя
            var question = await _context.Questions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == dto.QuestionId && q.Quiz.UserId == userId);
                
            if (question == null)
                return NotFound("Question not found or access denied");

            var answer = new Answer
            {
                Text = dto.Text,
                IsCorrect = dto.IsCorrect,
                QuestionId = dto.QuestionId,
                OrderIndex = await _context.Answers
                    .Where(a => a.QuestionId == dto.QuestionId)
                    .CountAsync()
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnswer), new { id = answer.Id }, answer);
        }

        /// <summary>
        /// Получить ответ по ID
        /// </summary>
        [HttpGet("answers/{id}")]
        public async Task<ActionResult<Answer>> GetAnswer(int id)
        {
            var answer = await _context.Answers.FindAsync(id);

            if (answer == null)
                return NotFound();

            return Ok(answer);
        }

        /// <summary>
        /// Обновить ответ (только для владельца квиза)
        /// </summary>
        [HttpPut("answers/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAnswer(int id, UpdateAnswerDto dto)
        {
            var userId = GetUserId()!;
            
            var answer = await _context.Answers
                .Include(a => a.Question)
                    .ThenInclude(q => q.Quiz)
                .FirstOrDefaultAsync(a => a.Id == id && a.Question.Quiz.UserId == userId);
                
            if (answer == null)
                return NotFound("Answer not found or access denied");

            answer.Text = dto.Text;
            answer.IsCorrect = dto.IsCorrect;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить ответ (только для владельца квиза)
        /// </summary>
        [HttpDelete("answers/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var userId = GetUserId()!;
            
            var answer = await _context.Answers
                .Include(a => a.Question)
                    .ThenInclude(q => q.Quiz)
                .FirstOrDefaultAsync(a => a.Id == id && a.Question.Quiz.UserId == userId);
                
            if (answer == null)
                return NotFound("Answer not found or access denied");

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
