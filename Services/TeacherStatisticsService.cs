using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Repositories;

namespace UniStart.Services;

public interface ITeacherStatisticsService
{
    Task<TeacherStudentsResult> GetStudentsAsync(TeacherStudentsFilter filter);
    Task<StudentDetailedStats?> GetStudentStatsAsync(string teacherId, string studentId);
}

public class TeacherStatisticsService : ITeacherStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TeacherStatisticsService> _logger;

    public TeacherStatisticsService(
        IUnitOfWork unitOfWork,
        ILogger<TeacherStatisticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TeacherStudentsResult> GetStudentsAsync(TeacherStudentsFilter filter)
    {
        // Получаем ID квизов преподавателя
        var teacherQuizIds = await _unitOfWork.Repository<Models.Quizzes.Quiz>()
            .Query()
            .AsNoTracking()
            .Where(q => q.UserId == filter.TeacherId)
            .Select(q => q.Id)
            .ToListAsync();

        if (teacherQuizIds.Count == 0)
        {
            return new TeacherStudentsResult
            {
                TotalStudents = 0,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = 0,
                Students = new List<StudentStats>()
            };
        }

        // Базовый запрос попыток
        var query = _unitOfWork.Repository<Models.Quizzes.UserQuizAttempt>()
            .Query()
            .AsNoTracking()
            .Where(qa => teacherQuizIds.Contains(qa.QuizId));

        if (filter.QuizId.HasValue)
            query = query.Where(qa => qa.QuizId == filter.QuizId.Value);

        var attempts = await query
            .Where(qa => qa.CompletedAt != null)
            .ToListAsync();

        if (attempts.Count == 0)
        {
            return new TeacherStudentsResult
            {
                TotalStudents = 0,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = 0,
                Students = new List<StudentStats>()
            };
        }

        var userIds = attempts.Select(a => a.UserId).Distinct().ToList();

        // Загружаем пользователей
        var users = await _unitOfWork.Repository<Models.Core.ApplicationUser>()
            .Query()
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        // Загружаем статистику по экзаменам
        var examAttempts = await _unitOfWork.Repository<Models.Exams.UserExamAttempt>()
            .Query()
            .AsNoTracking()
            .Where(ea => userIds.Contains(ea.UserId) && ea.CompletedAt != null)
            .ToListAsync();

        var examStatsDict = examAttempts
            .GroupBy(ea => ea.UserId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    ExamsTaken = g.Select(ea => ea.ExamId).Distinct().Count(),
                    LastExamDate = g.Max(ea => ea.CompletedAt)
                });

        // Загружаем статистику по карточкам
        var flashcardProgresses = await _unitOfWork.Repository<Models.Flashcards.UserFlashcardProgress>()
            .Query()
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId) && p.LastReviewedAt != null)
            .ToListAsync();

        var flashcardStatsDict = flashcardProgresses
            .GroupBy(p => p.UserId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    CardsStudied = g.Count(),
                    LastCardDate = g.Max(p => (DateTime?)p.LastReviewedAt)
                });

        // Формируем статистику по студентам
        var studentStats = attempts
            .GroupBy(qa => qa.UserId)
            .Select(g =>
            {
                var lastQuizDate = g.Max(a => a.CompletedAt);
                var examStats = examStatsDict.GetValueOrDefault(g.Key);
                var flashcardStats = flashcardStatsDict.GetValueOrDefault(g.Key);

                var lastActivityDate = new[] { 
                    lastQuizDate, 
                    examStats?.LastExamDate, 
                    flashcardStats?.LastCardDate 
                }
                .Where(d => d.HasValue)
                .DefaultIfEmpty()
                .Max();

                var user = users.GetValueOrDefault(g.Key);

                return new StudentStats
                {
                    UserId = g.Key,
                    Email = user?.Email ?? "Unknown",
                    UserName = user?.UserName ?? "Unknown",
                    FirstName = user?.FirstName ?? "",
                    LastName = user?.LastName ?? "",
                    TotalAttempts = g.Count(),
                    AverageScore = Math.Round(g.Average(a => a.Percentage), 2),
                    BestScore = g.Max(a => a.Percentage),
                    LastAttemptDate = lastQuizDate,
                    LastActivityDate = lastActivityDate,
                    QuizzesTaken = g.Select(a => a.QuizId).Distinct().Count(),
                    ExamsTaken = examStats?.ExamsTaken ?? 0,
                    CardsStudied = flashcardStats?.CardsStudied ?? 0
                };
            })
            .ToList();

        // Фильтрация по поиску
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            studentStats = studentStats
                .Where(s =>
                    s.Email.ToLower().Contains(searchLower) ||
                    s.UserName.ToLower().Contains(searchLower) ||
                    (s.FirstName != null && s.FirstName.ToLower().Contains(searchLower)) ||
                    (s.LastName != null && s.LastName.ToLower().Contains(searchLower)))
                .ToList();
        }

        // Фильтрация по минимальному баллу
        if (filter.MinScore.HasValue)
        {
            studentStats = studentStats.Where(s => s.AverageScore >= filter.MinScore.Value).ToList();
        }

        // Сортировка
        studentStats = filter.SortBy?.ToLower() switch
        {
            "lastactivity" => filter.Desc
                ? studentStats.OrderByDescending(s => s.LastActivityDate).ToList()
                : studentStats.OrderBy(s => s.LastActivityDate).ToList(),
            "attempts" => filter.Desc
                ? studentStats.OrderByDescending(s => s.TotalAttempts).ToList()
                : studentStats.OrderBy(s => s.TotalAttempts).ToList(),
            "name" => filter.Desc
                ? studentStats.OrderByDescending(s => s.LastName).ThenByDescending(s => s.FirstName).ToList()
                : studentStats.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList(),
            _ => filter.Desc
                ? studentStats.OrderByDescending(s => s.AverageScore).ToList()
                : studentStats.OrderBy(s => s.AverageScore).ToList()
        };

        var totalStudents = studentStats.Count;

        // Пагинация
        studentStats = studentStats
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new TeacherStudentsResult
        {
            TotalStudents = totalStudents,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalStudents / (double)filter.PageSize),
            Students = studentStats
        };
    }

    public async Task<StudentDetailedStats?> GetStudentStatsAsync(string teacherId, string studentId)
    {
        // Получаем ID квизов преподавателя
        var teacherQuizIds = await _unitOfWork.Repository<Models.Quizzes.Quiz>()
            .Query()
            .AsNoTracking()
            .Where(q => q.UserId == teacherId)
            .Select(q => q.Id)
            .ToListAsync();

        if (teacherQuizIds.Count == 0)
            return null;

        // Статистика по квизам
        var quizAttempts = await _unitOfWork.Repository<Models.Quizzes.UserQuizAttempt>()
            .Query()
            .AsNoTracking()
            .Where(qa => qa.UserId == studentId && teacherQuizIds.Contains(qa.QuizId) && qa.CompletedAt != null)
            .ToListAsync();

        // Статистика по экзаменам (все экзамены студента)
        var examAttempts = await _unitOfWork.Repository<Models.Exams.UserExamAttempt>()
            .Query()
            .AsNoTracking()
            .Where(ea => ea.UserId == studentId && ea.CompletedAt != null)
            .ToListAsync();

        // Статистика по flashcard sets
        var completedSetsCount = await _unitOfWork.Repository<Models.Flashcards.UserFlashcardSetAccess>()
            .Query()
            .AsNoTracking()
            .Where(a => a.UserId == studentId && a.IsCompleted)
            .CountAsync();

        var reviewedCardsCount = await _unitOfWork.Repository<Models.Flashcards.UserFlashcardProgress>()
            .Query()
            .AsNoTracking()
            .Where(p => p.UserId == studentId && p.Repetitions > 0)
            .CountAsync();

        var masteredCardsCount = await _unitOfWork.Repository<Models.Flashcards.UserFlashcardProgress>()
            .Query()
            .AsNoTracking()
            .Where(p => p.UserId == studentId && p.IsMastered)
            .CountAsync();

        // Загружаем квизы для детальной информации
        var quizIds = quizAttempts.Select(qa => qa.QuizId).Distinct().ToList();
        var quizzesDict = await _unitOfWork.Repository<Models.Quizzes.Quiz>()
            .Query()
            .AsNoTracking()
            .Where(q => quizIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        var examsDict = await _unitOfWork.Repository<Models.Exams.Exam>()
            .Query()
            .AsNoTracking()
            .Where(e => examAttempts.Select(ea => ea.ExamId).Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        // Статистика по flashcard sets с прогрессом
        var flashcardSetsAccess = await _unitOfWork.Repository<Models.Flashcards.UserFlashcardSetAccess>()
            .Query()
            .AsNoTracking()
            .Where(a => a.UserId == studentId)
            .Include(a => a.FlashcardSet)
            .ToListAsync();

        var flashcardSetIds = flashcardSetsAccess.Select(a => a.FlashcardSetId).ToList();
        var flashcardProgressBySet = await _unitOfWork.Repository<Models.Flashcards.UserFlashcardProgress>()
            .Query()
            .AsNoTracking()
            .Where(p => p.UserId == studentId)
            .Include(p => p.Flashcard)
            .ToListAsync();

        var flashcardSetStats = flashcardSetsAccess.Select(access =>
        {
            var progresses = flashcardProgressBySet
                .Where(p => p.Flashcard.FlashcardSetId == access.FlashcardSetId)
                .ToList();

            return new FlashcardSetProgressStats
            {
                SetId = access.FlashcardSetId,
                SetName = access.FlashcardSet?.Title ?? "Unknown",
                TotalCards = access.FlashcardSet?.Flashcards?.Count ?? 0,
                ReviewedCards = progresses.Count(p => p.Repetitions > 0),
                MasteredCards = progresses.Count(p => p.IsMastered),
                LastAccessDate = access.LastAccessedAt,
                IsCompleted = access.IsCompleted
            };
        }).ToList();

        return new StudentDetailedStats
        {
            QuizAttempts = quizAttempts.Count,
            AverageQuizScore = quizAttempts.Any() ? Math.Round(quizAttempts.Average(qa => qa.Percentage), 2) : 0,
            BestQuizScore = quizAttempts.Any() ? quizAttempts.Max(qa => qa.Percentage) : 0,
            QuizzesTaken = quizAttempts.Select(qa => qa.QuizId).Distinct().Count(),
            ExamAttempts = examAttempts.Count,
            AverageExamScore = examAttempts.Any() ? Math.Round(examAttempts.Average(ea => ea.Percentage), 2) : 0,
            BestExamScore = examAttempts.Any() ? examAttempts.Max(ea => ea.Percentage) : 0,
            ExamsPassed = examAttempts.Count(ea => ea.Passed),
            CompletedFlashcardSets = completedSetsCount,
            ReviewedCards = reviewedCardsCount,
            MasteredCards = masteredCardsCount,
            QuizHistory = quizAttempts.Select(qa => new QuizAttemptInfo
            {
                QuizId = qa.QuizId,
                QuizTitle = quizzesDict.GetValueOrDefault(qa.QuizId)?.Title ?? "Unknown",
                Score = qa.Percentage,
                CompletedAt = qa.CompletedAt
            }).OrderByDescending(q => q.CompletedAt).ToList(),
            ExamHistory = examAttempts.Select(ea => new ExamAttemptInfo
            {
                ExamId = ea.ExamId,
                ExamTitle = examsDict.GetValueOrDefault(ea.ExamId)?.Title ?? "Unknown",
                Score = ea.Percentage,
                IsPassed = ea.Passed,
                CompletedAt = ea.CompletedAt
            }).OrderByDescending(e => e.CompletedAt).ToList(),
            FlashcardSetProgress = flashcardSetStats
        };
    }
}

// DTOs
public class TeacherStudentsFilter
{
    public string TeacherId { get; set; } = string.Empty;
    public int? QuizId { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; } = "averageScore";
    public bool Desc { get; set; } = true;
    public double? MinScore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TeacherStudentsResult
{
    public int TotalStudents { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<StudentStats> Students { get; set; } = new();
}

public class StudentStats
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int TotalAttempts { get; set; }
    public double AverageScore { get; set; }
    public double BestScore { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int QuizzesTaken { get; set; }
    public int ExamsTaken { get; set; }
    public int CardsStudied { get; set; }
}

public class StudentDetailedStats
{
    public int QuizAttempts { get; set; }
    public double AverageQuizScore { get; set; }
    public double BestQuizScore { get; set; }
    public int QuizzesTaken { get; set; }
    public int ExamAttempts { get; set; }
    public double AverageExamScore { get; set; }
    public double BestExamScore { get; set; }
    public int ExamsPassed { get; set; }
    public int CompletedFlashcardSets { get; set; }
    public int ReviewedCards { get; set; }
    public int MasteredCards { get; set; }
    public List<QuizAttemptInfo> QuizHistory { get; set; } = new();
    public List<ExamAttemptInfo> ExamHistory { get; set; } = new();
    public List<FlashcardSetProgressStats> FlashcardSetProgress { get; set; } = new();
}

public class QuizAttemptInfo
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ExamAttemptInfo
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class FlashcardSetProgressStats
{
    public int SetId { get; set; }
    public string SetName { get; set; } = string.Empty;
    public int TotalCards { get; set; }
    public int ReviewedCards { get; set; }
    public int MasteredCards { get; set; }
    public DateTime? LastAccessDate { get; set; }
    public bool IsCompleted { get; set; }
}
