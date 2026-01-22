using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs;

/// <summary>
/// DTO для создания/обновления предпочтений пользователя (Onboarding)
/// </summary>
public class UserPreferencesDto
{
    // ========== ЦЕЛИ ОБУЧЕНИЯ ==========
    
    /// <summary>
    /// Основная цель обучения
    /// </summary>
    [Required(ErrorMessage = "Укажите цель обучения")]
    public string LearningGoal { get; set; } = "SelfStudy"; // ENT, University, SelfStudy, Professional
    
    /// <summary>
    /// Целевой тип экзамена
    /// </summary>
    public string? TargetExamType { get; set; }
    
    /// <summary>
    /// ID целевого университета
    /// </summary>
    public int? TargetUniversityId { get; set; }
    
    /// <summary>
    /// Карьерная цель
    /// </summary>
    [StringLength(200)]
    public string? CareerGoal { get; set; }
    
    // ========== ГЕОГРАФИЧЕСКИЕ ПРЕДПОЧТЕНИЯ ==========
    
    /// <summary>
    /// Предпочитаемая страна для обучения
    /// </summary>
    [StringLength(100)]
    public string? PreferredCountry { get; set; }
    
    /// <summary>
    /// Предпочитаемый город для обучения
    /// </summary>
    [StringLength(100)]
    public string? PreferredCity { get; set; }
    
    /// <summary>
    /// Готовность к переезду
    /// </summary>
    public bool WillingToRelocate { get; set; } = false;
    
    // ========== ФИНАНСОВЫЕ ПРЕДПОЧТЕНИЯ ==========
    
    /// <summary>
    /// Максимальный бюджет на обучение в год
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxBudgetPerYear { get; set; }
    
    /// <summary>
    /// Интересуют ли гранты/стипендии
    /// </summary>
    public bool InterestedInScholarships { get; set; } = true;
    
    // ========== ЯЗЫКОВЫЕ ПРЕДПОЧТЕНИЯ ==========
    
    /// <summary>
    /// Предпочитаемые языки обучения
    /// </summary>
    public List<string> PreferredLanguages { get; set; } = new() { "Russian" };
    
    /// <summary>
    /// Уровень английского (A1, A2, B1, B2, C1, C2)
    /// </summary>
    [StringLength(10)]
    public string? EnglishLevel { get; set; }
    
    // ========== ПРЕДМЕТЫ ==========
    
    /// <summary>
    /// Список ID интересующих предметов
    /// </summary>
    [Required(ErrorMessage = "Выберите хотя бы один предмет")]
    [MinLength(1, ErrorMessage = "Выберите хотя бы один предмет")]
    public List<int> InterestedSubjectIds { get; set; } = new();
    
    /// <summary>
    /// Список ID сильных предметов
    /// </summary>
    public List<int> StrongSubjectIds { get; set; } = new();
    
    /// <summary>
    /// Список ID слабых предметов
    /// </summary>
    public List<int> WeakSubjectIds { get; set; } = new();
    
    /// <summary>
    /// Предпочитает карточки
    /// </summary>
    public bool PrefersFlashcards { get; set; } = true;
    
    /// <summary>
    /// Предпочитает квизы
    /// </summary>
    public bool PrefersQuizzes { get; set; } = true;
    
    /// <summary>
    /// Предпочитает экзамены
    /// </summary>
    public bool PrefersExams { get; set; } = false;
    
    /// <summary>
    /// Предпочитаемая сложность (1=Easy, 2=Medium, 3=Hard)
    /// </summary>
    [Range(1, 3, ErrorMessage = "Сложность должна быть от 1 до 3")]
    public int PreferredDifficulty { get; set; } = 2;
    
    /// <summary>
    /// Планируемое время обучения в день (минуты)
    /// </summary>
    [Range(5, 480, ErrorMessage = "Время должно быть от 5 до 480 минут")]
    public int DailyStudyTimeMinutes { get; set; } = 30;
    
    /// <summary>
    /// Предпочитаемое время обучения
    /// </summary>
    public string? PreferredStudyTime { get; set; }
    
    /// <summary>
    /// Дни недели для обучения
    /// </summary>
    public List<string> StudyDays { get; set; } = new() { "Mon", "Tue", "Wed", "Thu", "Fri" };
    
    /// <summary>
    /// Уровень мотивации (1-5)
    /// </summary>
    [Range(1, 5, ErrorMessage = "Мотивация должна быть от 1 до 5")]
    public int MotivationLevel { get; set; } = 3;
    
    /// <summary>
    /// Нужны ли напоминания
    /// </summary>
    public bool NeedsReminders { get; set; } = true;
}

/// <summary>
/// DTO для получения предпочтений пользователя
/// </summary>
public class UserPreferencesResponseDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LearningGoal { get; set; } = string.Empty;
    public string? TargetExamType { get; set; }
    public int? TargetUniversityId { get; set; }
    public string? TargetUniversityName { get; set; }
    public string? CareerGoal { get; set; }
    
    // ========== ГЕОГРАФИЧЕСКИЕ ПРЕДПОЧТЕНИЯ ==========
    public string? PreferredCountry { get; set; }
    public string? PreferredCity { get; set; }
    public bool WillingToRelocate { get; set; }
    
    // ========== ФИНАНСОВЫЕ ПРЕДПОЧТЕНИЯ ==========
    public decimal? MaxBudgetPerYear { get; set; }
    public bool InterestedInScholarships { get; set; }
    
    // ========== ЯЗЫКОВЫЕ ПРЕДПОЧТЕНИЯ ==========
    public List<string> PreferredLanguages { get; set; } = new();
    public string? EnglishLevel { get; set; }
    
    // ========== ПРЕДМЕТЫ ==========
    
    public List<SubjectDto> InterestedSubjects { get; set; } = new();
    public List<SubjectDto> StrongSubjects { get; set; } = new();
    public List<SubjectDto> WeakSubjects { get; set; } = new();
    
    public bool PrefersFlashcards { get; set; }
    public bool PrefersQuizzes { get; set; }
    public bool PrefersExams { get; set; }
    public int PreferredDifficulty { get; set; }
    public int DailyStudyTimeMinutes { get; set; }
    public string? PreferredStudyTime { get; set; }
    public List<string> StudyDays { get; set; } = new();
    public int MotivationLevel { get; set; }
    public bool NeedsReminders { get; set; }
    public bool OnboardingCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Простой DTO для предмета
/// </summary>
public class SubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
