using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Controllers.Student
{
    [ApiController]
    [Route("api/student")]
    [Authorize]
    public class StudentContentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentContentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User) 
            ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

        /// <summary>
        /// Получить все свои наборы карточек
        /// </summary>
        [HttpGet("my-flashcard-sets")]
        public async Task<ActionResult<IEnumerable<FlashcardSetDto>>> GetMyFlashcardSets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();

            var flashcardSets = await _context.FlashcardSets
                .Where(fs => fs.UserId == userId)
                .Include(fs => fs.Flashcards)
                .OrderByDescending(fs => fs.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(fs => new FlashcardSetDto
                {
                    Id = fs.Id,
                    Title = fs.Title,
                    Description = fs.Description,
                    Subject = fs.Subject,
                    CardCount = fs.Flashcards.Count,
                    CreatedAt = fs.CreatedAt,
                    UpdatedAt = fs.UpdatedAt
                })
                .ToListAsync();

            return Ok(flashcardSets);
        }

        /// <summary>
        /// Получить все попытки прохождения квизов
        /// </summary>
        [HttpGet("my-quiz-attempts")]
        public async Task<ActionResult<object>> GetMyQuizAttempts(
            [FromQuery] int? quizId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();

            var query = _context.UserQuizAttempts
                .Include(qa => qa.Quiz)
                .Where(qa => qa.UserId == userId);

            if (quizId.HasValue)
                query = query.Where(qa => qa.QuizId == quizId.Value);

            var attempts = await query
                .OrderByDescending(qa => qa.CompletedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(qa => new
                {
                    qa.Id,
                    QuizId = qa.Quiz.Id,
                    QuizTitle = qa.Quiz.Title,
                    qa.Score,
                    qa.MaxScore,
                    qa.Percentage,
                    qa.TimeSpentSeconds,
                    qa.CompletedAt
                })
                .ToListAsync();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                Attempts = attempts
            });
        }

        /// <summary>
        /// Получить доступные публичные квизы
        /// </summary>
        [HttpGet("available-quizzes")]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetAvailableQuizzes(
            [FromQuery] string? subject = null,
            [FromQuery] string? difficulty = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.Quizzes
                .Include(q => q.Questions)
                .Where(q => q.IsPublic && q.IsPublished); // Только публичные и опубликованные квизы

            if (!string.IsNullOrWhiteSpace(subject))
                query = query.Where(q => q.Subject.Contains(subject));

            if (!string.IsNullOrWhiteSpace(difficulty))
                query = query.Where(q => q.Difficulty == difficulty);

            var quizzes = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuizDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Subject = q.Subject,
                    Difficulty = q.Difficulty,
                    TimeLimit = q.TimeLimit,
                    QuestionCount = q.Questions.Count,
                    IsPublic = q.IsPublic,
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        /// <summary>
        /// Получить доступные публичные экзамены
        /// </summary>
        [HttpGet("available-exams")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableExams(
            [FromQuery] string? subject = null,
            [FromQuery] string? difficulty = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.Exams
                .Include(e => e.Questions)
                .Include(e => e.Subjects)
                .Where(e => e.IsPublic && e.IsPublished); // Только публичные и опубликованные экзамены

            if (!string.IsNullOrWhiteSpace(subject))
                query = query.Where(e => e.Subjects.Any(s => s.Name.Contains(subject)));

            if (!string.IsNullOrWhiteSpace(difficulty))
                query = query.Where(e => e.Difficulty == difficulty);

            var exams = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Difficulty,
                    e.TimeLimit,
                    QuestionCount = e.Questions.Count,
                    e.IsPublic,
                    e.CreatedAt
                })
                .ToListAsync();

            return Ok(exams);
        }

        /// <summary>
        /// Получить доступные публичные наборы карточек
        /// </summary>
        [HttpGet("available-flashcard-sets")]
        public async Task<ActionResult<IEnumerable<FlashcardSetDto>>> GetAvailableFlashcardSets(
            [FromQuery] string? subject = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.FlashcardSets
                .Include(fs => fs.Flashcards)
                .Where(fs => fs.IsPublic && fs.IsPublished); // Только публичные и опубликованные наборы

            if (!string.IsNullOrWhiteSpace(subject))
                query = query.Where(fs => fs.Subject.Contains(subject));

            var flashcardSets = await query
                .OrderByDescending(fs => fs.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(fs => new FlashcardSetDto
                {
                    Id = fs.Id,
                    Title = fs.Title,
                    Description = fs.Description,
                    Subject = fs.Subject,
                    CardCount = fs.Flashcards.Count,
                    CreatedAt = fs.CreatedAt,
                    UpdatedAt = fs.UpdatedAt
                })
                .ToListAsync();

            return Ok(flashcardSets);
        }
    }
}
