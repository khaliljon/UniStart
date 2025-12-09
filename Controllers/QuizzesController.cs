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

            var userId = GetUserId();
            var isOwnerOrAdmin = quiz.UserId == userId || User.IsInRole("Admin") || User.IsInRole("Teacher");

            var quizDto = new QuizDetailDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimit = quiz.TimeLimit,
                Subject = quiz.Subject,
                Difficulty = quiz.Difficulty,
                IsPublic = quiz.IsPublic,
                IsPublished = quiz.IsPublished,
                IsLearningMode = quiz.IsLearningMode,
                Questions = quiz.Questions.OrderBy(q => q.OrderIndex).Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Points = q.Points,
                    ImageUrl = q.ImageUrl,
                    Explanation = q.Explanation,
                    Answers = q.Answers.OrderBy(a => a.OrderIndex).Select(a => new QuizAnswerDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = quiz.IsLearningMode || isOwnerOrAdmin ? a.IsCorrect : null // В режиме обучения показываем всем
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
        /// Обновить тест (только свои) включая вопросы и ответы
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz(int id, UpdateQuizDto dto)
        {
            var userId = GetUserId()!;
            
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Answers)
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);
                
            if (quiz == null)
                return NotFound();

            // Обновляем базовую информацию
            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.TimeLimit = dto.TimeLimit;
            quiz.Subject = dto.Subject;
            quiz.Difficulty = dto.Difficulty;
            quiz.IsPublic = dto.IsPublic;
            quiz.IsPublished = dto.IsPublished;
            quiz.UpdatedAt = DateTime.UtcNow;

            // Удаляем старые вопросы и ответы
            _context.QuizQuestions.RemoveRange(quiz.Questions);

            // Добавляем новые вопросы с ответами
            foreach (var questionDto in dto.Questions)
            {
                var QuizQuestion = new QuizQuestion
                {
                    Text = questionDto.Text,
                    Points = questionDto.Points,
                    Explanation = questionDto.Explanation,
                    OrderIndex = questionDto.Order,
                    QuizId = quiz.Id
                };

                foreach (var answerDto in questionDto.Answers)
                {
                    QuizQuestion.Answers.Add(new QuizAnswer
                    {
                        Text = answerDto.Text,
                        IsCorrect = answerDto.IsCorrect,
                        OrderIndex = answerDto.Order
                    });
                }

                quiz.Questions.Add(QuizQuestion);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить тест (свои или любые для админа)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var userId = GetUserId()!;
            var isAdmin = User.IsInRole("Admin");
            
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && (q.UserId == userId || isAdmin));
                
            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Опубликовать квиз
        /// </summary>
        [HttpPatch("{id}/publish")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> PublishQuiz(int id)
        {
            var userId = GetUserId()!;
            var isAdmin = User.IsInRole("Admin");
            
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && (q.UserId == userId || isAdmin));
                
            if (quiz == null)
                return NotFound();

            quiz.IsPublished = true;
            await _context.SaveChangesAsync();
            return Ok(new { isPublished = quiz.IsPublished });
        }

        /// <summary>
        /// Снять квиз с публикации
        /// </summary>
        [HttpPatch("{id}/unpublish")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> UnpublishQuiz(int id)
        {
            var userId = GetUserId()!;
            var isAdmin = User.IsInRole("Admin");
            
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && (q.UserId == userId || isAdmin));
                
            if (quiz == null)
                return NotFound();

            quiz.IsPublished = false;
            await _context.SaveChangesAsync();
            return Ok(new { isPublished = quiz.IsPublished });
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
            var questionResults = new List<QuizQuestionResultDto>();
            int totalScore = 0;
            int maxScore = quiz.Questions.Sum(q => q.Points);

            foreach (var QuizQuestion in quiz.Questions)
            {
                var correctAnswerIds = QuizQuestion.Answers
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .OrderBy(id => id)
                    .ToList();

                var userAnswerIds = dto.UserAnswers.ContainsKey(QuizQuestion.Id)
                    ? dto.UserAnswers[QuizQuestion.Id].OrderBy(id => id).ToList()
                    : new List<int>();

                bool isCorrect = correctAnswerIds.SequenceEqual(userAnswerIds);
                int pointsEarned = isCorrect ? QuizQuestion.Points : 0;
                totalScore += pointsEarned;

                questionResults.Add(new QuizQuestionResultDto
                {
                    QuestionId = QuizQuestion.Id,
                    QuestionText = QuizQuestion.Text,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned,
                    MaxPoints = QuizQuestion.Points,
                    CorrectAnswerIds = correctAnswerIds,
                    UserAnswerIds = userAnswerIds,
                    Explanation = QuizQuestion.Explanation
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
            foreach (var QuizQuestion in quiz.Questions.OrderBy(q => q.OrderIndex))
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
                        if (userAnswers == null || !userAnswers.ContainsKey(QuizQuestion.Id))
                            continue;

                        totalAnswers++;

                        var correctAnswerIds = QuizQuestion.Answers
                            .Where(a => a.IsCorrect)
                            .Select(a => a.Id)
                            .OrderBy(id => id)
                            .ToList();

                        var userAnswerIds = userAnswers[QuizQuestion.Id].OrderBy(id => id).ToList();

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
                    questionId = QuizQuestion.Id,
                    questionText = QuizQuestion.Text,
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
        public async Task<ActionResult<QuizQuestion>> CreateQuestion(CreateQuizQuestionDto dto)
        {
            var userId = GetUserId()!;
            
            // Проверяем, что квиз принадлежит текущему пользователю
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId && q.UserId == userId);
                
            if (quiz == null)
                return NotFound("Quiz not found or access denied");

            var QuizQuestion = new QuizQuestion
            {
                Text = dto.Text,
                Points = dto.Points,
                ImageUrl = dto.ImageUrl,
                Explanation = dto.Explanation,
                QuizId = dto.QuizId,
                OrderIndex = await _context.QuizQuestions
                    .Where(q => q.QuizId == dto.QuizId)
                    .CountAsync()
            };

            _context.QuizQuestions.Add(QuizQuestion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuestion), new { id = QuizQuestion.Id }, QuizQuestion);
        }

        /// <summary>
        /// Получить вопрос по ID
        /// </summary>
        [HttpGet("questions/{id}")]
        public async Task<ActionResult<QuizQuestion>> GetQuestion(int id)
        {
            var QuizQuestion = await _context.QuizQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (QuizQuestion == null)
                return NotFound();

            return Ok(QuizQuestion);
        }

        /// <summary>
        /// Обновить вопрос (только для владельца квиза)
        /// </summary>
        [HttpPut("questions/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestion(int id, UpdateQuizQuestionDto dto)
        {
            var userId = GetUserId()!;
            
            var QuizQuestion = await _context.QuizQuestions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == userId);
                
            if (QuizQuestion == null)
                return NotFound("QuizQuestion not found or access denied");

            QuizQuestion.Text = dto.Text;
            QuizQuestion.Points = dto.Points;
            QuizQuestion.ImageUrl = dto.ImageUrl;
            QuizQuestion.Explanation = dto.Explanation;

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
            
            var QuizQuestion = await _context.QuizQuestions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == userId);
                
            if (QuizQuestion == null)
                return NotFound("QuizQuestion not found or access denied");

            _context.QuizQuestions.Remove(QuizQuestion);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============ ANSWERS CRUD ============

        /// <summary>
        /// Создать новый ответ для вопроса (только для владельца квиза)
        /// </summary>
        [HttpPost("answers")]
        [Authorize]
        public async Task<ActionResult<QuizAnswer>> CreateAnswer(CreateQuizAnswerDto dto)
        {
            var userId = GetUserId()!;
            
            // Проверяем, что вопрос принадлежит квизу пользователя
            var QuizQuestion = await _context.QuizQuestions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == dto.QuestionId && q.Quiz.UserId == userId);
                
            if (QuizQuestion == null)
                return NotFound("QuizQuestion not found or access denied");

            var QuizAnswer = new QuizAnswer
            {
                Text = dto.Text,
                IsCorrect = dto.IsCorrect,
                QuestionId = dto.QuestionId,
                OrderIndex = await _context.QuizAnswers
                    .Where(a => a.QuestionId == dto.QuestionId)
                    .CountAsync()
            };

            _context.QuizAnswers.Add(QuizAnswer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnswer), new { id = QuizAnswer.Id }, QuizAnswer);
        }

        /// <summary>
        /// Получить ответ по ID
        /// </summary>
        [HttpGet("answers/{id}")]
        public async Task<ActionResult<QuizAnswer>> GetAnswer(int id)
        {
            var QuizAnswer = await _context.QuizAnswers.FindAsync(id);

            if (QuizAnswer == null)
                return NotFound();

            return Ok(QuizAnswer);
        }

        /// <summary>
        /// Обновить ответ (только для владельца квиза)
        /// </summary>
        [HttpPut("answers/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAnswer(int id, UpdateQuizAnswerDto dto)
        {
            var userId = GetUserId()!;
            
            var QuizAnswer = await _context.QuizAnswers
                .Include(a => a.Question)
                    .ThenInclude(q => q.Quiz)
                .FirstOrDefaultAsync(a => a.Id == id && a.Question.Quiz.UserId == userId);
                
            if (QuizAnswer == null)
                return NotFound("QuizAnswer not found or access denied");

            QuizAnswer.Text = dto.Text;
            QuizAnswer.IsCorrect = dto.IsCorrect;

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
            
            var QuizAnswer = await _context.QuizAnswers
                .Include(a => a.Question)
                    .ThenInclude(q => q.Quiz)
                .FirstOrDefaultAsync(a => a.Id == id && a.Question.Quiz.UserId == userId);
                
            if (QuizAnswer == null)
                return NotFound("QuizAnswer not found or access denied");

            _context.QuizAnswers.Remove(QuizAnswer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============ QUIZ ATTEMPTS ============

        /// <summary>
        /// Начать попытку прохождения квиза (создать запись)
        /// </summary>
        [HttpPost("{id}/attempts/start")]
        [Authorize]
        public async Task<ActionResult<object>> StartQuizAttempt(int id)
        {
            var userId = GetUserId()!;
            
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound("Quiz not found");

            if (!quiz.IsPublished)
                return BadRequest("Quiz is not published");

            var attempt = new UserQuizAttempt
            {
                UserId = userId,
                QuizId = id,
                StartedAt = DateTime.UtcNow,
                Score = 0,
                MaxScore = 0,
                Percentage = 0,
                TimeSpentSeconds = 0,
                UserAnswersJson = "{}"
            };

            _context.UserQuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return Ok(new { id = attempt.Id });
        }

        /// <summary>
        /// Отправить ответы и завершить попытку
        /// </summary>
        [HttpPost("{id}/attempts/{attemptId}/submit")]
        [Authorize]
        public async Task<ActionResult> SubmitQuizAttempt(int id, int attemptId, [FromBody] SubmitQuizDto dto)
        {
            var userId = GetUserId()!;
            
            var attempt = await _context.UserQuizAttempts
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qu => qu.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.QuizId == id && a.UserId == userId);

            if (attempt == null)
                return NotFound("Attempt not found or access denied");

            if (attempt.CompletedAt != null)
                return BadRequest("Attempt already completed");

            // Calculate score
            int totalScore = 0;
            int maxScore = 0;
            int correctQuestions = 0;

            Console.WriteLine($"=== Quiz Submission Debug ===");
            Console.WriteLine($"User answers received: {JsonSerializer.Serialize(dto.UserAnswers)}");
            Console.WriteLine($"Total questions in quiz: {attempt.Quiz.Questions.Count}");

            foreach (var question in attempt.Quiz.Questions)
            {
                maxScore += question.Points;
                
                Console.WriteLine($"\nQuestion {question.Id}: {question.Text}");

                if (dto.UserAnswers.TryGetValue(question.Id, out var userAnswerIds))
                {
                    var correctAnswerIds = question.Answers
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .OrderBy(x => x)
                        .ToList();

                    var sortedUserAnswers = userAnswerIds.OrderBy(x => x).ToList();
                    
                    Console.WriteLine($"  Correct answer IDs: [{string.Join(", ", correctAnswerIds)}]");
                    Console.WriteLine($"  User answer IDs: [{string.Join(", ", sortedUserAnswers)}]");

                    // Check if user answers match correct answers exactly
                    if (correctAnswerIds.SequenceEqual(sortedUserAnswers))
                    {
                        totalScore += question.Points;
                        correctQuestions++;
                        Console.WriteLine($"  ✓ CORRECT! Added {question.Points} points");
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ WRONG!");
                    }
                }
                else
                {
                    Console.WriteLine($"  - No answer provided");
                }
            }
            
            Console.WriteLine($"\nFinal score: {totalScore}/{maxScore} ({correctQuestions}/{attempt.Quiz.Questions.Count} correct)");
            Console.WriteLine($"=== End Debug ===\n");

            // Update attempt
            attempt.Score = totalScore;
            attempt.MaxScore = maxScore;
            attempt.Percentage = maxScore > 0 ? (double)totalScore / maxScore * 100 : 0;
            attempt.TimeSpentSeconds = dto.TimeSpentSeconds;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.UserAnswersJson = JsonSerializer.Serialize(dto.UserAnswers);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                score = totalScore,
                maxScore = maxScore,
                percentage = attempt.Percentage,
                passed = attempt.Percentage >= 70,
                correctQuestions = correctQuestions,
                totalQuestions = attempt.Quiz.Questions.Count
            });
        }
    }
}
