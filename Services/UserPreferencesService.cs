using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Learning;
using UniStart.Repositories;

namespace UniStart.Services;

/// <summary>
/// Сервис для управления предпочтениями пользователей
/// </summary>
public class UserPreferencesService : IUserPreferencesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserPreferencesService> _logger;

    public UserPreferencesService(
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        ILogger<UserPreferencesService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    public async Task<UserPreferencesResponseDto?> GetUserPreferencesAsync(string userId)
    {
        try
        {
            var preferences = await _context.UserPreferences
                .Include(up => up.InterestedSubjects)
                .Include(up => up.StrongSubjects)
                .Include(up => up.WeakSubjects)
                .Include(up => up.TargetUniversity)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (preferences == null)
                return null;

            var studyDays = string.IsNullOrEmpty(preferences.StudyDaysJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(preferences.StudyDaysJson) ?? new List<string>();

            return new UserPreferencesResponseDto
            {
                Id = preferences.Id,
                UserId = preferences.UserId,
                LearningGoal = preferences.LearningGoal,
                TargetExamType = preferences.TargetExamType,
                TargetUniversityId = preferences.TargetUniversityId,
                TargetUniversityName = preferences.TargetUniversity?.Name,
                CareerGoal = preferences.CareerGoal,
                InterestedSubjects = preferences.InterestedSubjects.Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                }).ToList(),
                StrongSubjects = preferences.StrongSubjects.Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                }).ToList(),
                WeakSubjects = preferences.WeakSubjects.Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                }).ToList(),
                PrefersFlashcards = preferences.PrefersFlashcards,
                PrefersQuizzes = preferences.PrefersQuizzes,
                PrefersExams = preferences.PrefersExams,
                PreferredDifficulty = preferences.PreferredDifficulty,
                DailyStudyTimeMinutes = preferences.DailyStudyTimeMinutes,
                PreferredStudyTime = preferences.PreferredStudyTime,
                StudyDays = studyDays,
                MotivationLevel = preferences.MotivationLevel,
                NeedsReminders = preferences.NeedsReminders,
                OnboardingCompleted = preferences.OnboardingCompleted,
                CreatedAt = preferences.CreatedAt,
                UpdatedAt = preferences.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предпочтений для User={UserId}", userId);
            throw;
        }
    }

    public async Task<UserPreferencesResponseDto> CreateOrUpdatePreferencesAsync(string userId, UserPreferencesDto dto)
    {
        try
        {
            var preferences = await _context.UserPreferences
                .Include(up => up.InterestedSubjects)
                .Include(up => up.StrongSubjects)
                .Include(up => up.WeakSubjects)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            var isNew = preferences == null;

            if (isNew)
            {
                preferences = new UserPreferences
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserPreferences.Add(preferences);
            }

            // Обновляем основные поля
            preferences.LearningGoal = dto.LearningGoal;
            preferences.TargetExamType = dto.TargetExamType;
            preferences.TargetUniversityId = dto.TargetUniversityId;
            preferences.CareerGoal = dto.CareerGoal;
            preferences.PrefersFlashcards = dto.PrefersFlashcards;
            preferences.PrefersQuizzes = dto.PrefersQuizzes;
            preferences.PrefersExams = dto.PrefersExams;
            preferences.PreferredDifficulty = dto.PreferredDifficulty;
            preferences.DailyStudyTimeMinutes = dto.DailyStudyTimeMinutes;
            preferences.PreferredStudyTime = dto.PreferredStudyTime;
            preferences.StudyDaysJson = JsonSerializer.Serialize(dto.StudyDays);
            preferences.MotivationLevel = dto.MotivationLevel;
            preferences.NeedsReminders = dto.NeedsReminders;
            preferences.UpdatedAt = DateTime.UtcNow;

            // Обновляем Many-to-Many связи с предметами
            if (!isNew)
            {
                preferences.InterestedSubjects.Clear();
                preferences.StrongSubjects.Clear();
                preferences.WeakSubjects.Clear();
            }

            var allSubjectIds = dto.InterestedSubjectIds
                .Concat(dto.StrongSubjectIds)
                .Concat(dto.WeakSubjectIds)
                .Distinct()
                .ToList();

            var subjects = await _context.Subjects
                .Where(s => allSubjectIds.Contains(s.Id))
                .ToListAsync();

            foreach (var subjectId in dto.InterestedSubjectIds)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == subjectId);
                if (subject != null)
                    preferences.InterestedSubjects.Add(subject);
            }

            foreach (var subjectId in dto.StrongSubjectIds)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == subjectId);
                if (subject != null)
                    preferences.StrongSubjects.Add(subject);
            }

            foreach (var subjectId in dto.WeakSubjectIds)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == subjectId);
                if (subject != null)
                    preferences.WeakSubjects.Add(subject);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                isNew ? "Созданы предпочтения для User={UserId}" : "Обновлены предпочтения для User={UserId}",
                userId);

            return (await GetUserPreferencesAsync(userId))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании/обновлении предпочтений для User={UserId}", userId);
            throw;
        }
    }

    public async Task<bool> HasCompletedOnboardingAsync(string userId)
    {
        try
        {
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId);

            return preferences?.OnboardingCompleted ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке Onboarding для User={UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CompleteOnboardingAsync(string userId)
    {
        try
        {
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (preferences == null)
                return false;

            preferences.OnboardingCompleted = true;
            preferences.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Onboarding завершен для User={UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при завершении Onboarding для User={UserId}", userId);
            return false;
        }
    }

    public async Task<List<SubjectDto>> GetRecommendedSubjectsAsync(string userId)
    {
        try
        {
            var preferences = await _context.UserPreferences
                .Include(up => up.InterestedSubjects)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (preferences == null || !preferences.InterestedSubjects.Any())
            {
                // Если нет предпочтений, возвращаем популярные предметы
                var popularSubjects = await _context.Subjects
                    .Where(s => s.IsActive)
                    .Take(10)
                    .ToListAsync();

                return popularSubjects.Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();
            }

            return preferences.InterestedSubjects.Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении рекомендуемых предметов для User={UserId}", userId);
            return new List<SubjectDto>();
        }
    }
}
