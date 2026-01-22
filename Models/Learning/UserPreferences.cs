using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniStart.Models.Core;
using UniStart.Models.Reference;

namespace UniStart.Models.Learning;

/// <summary>
/// Предпочтения пользователя для персонализации обучения и рекомендаций
/// Собираются при регистрации через Onboarding опрос
/// </summary>
public class UserPreferences
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// ID пользователя
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Навигация к пользователю
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
    
    // ========== ЦЕЛИ ОБУЧЕНИЯ ==========
    
    /// <summary>
    /// Основная цель обучения
    /// </summary>
    [Display(Name = "Цель обучения")]
    [Required]
    [StringLength(50)]
    public string LearningGoal { get; set; } = "SelfStudy"; // ENT, University, SelfStudy, Professional
    
    /// <summary>
    /// Целевой тип экзамена (если цель = ENT или University)
    /// </summary>
    [Display(Name = "Целевой экзамен")]
    [StringLength(100)]
    public string? TargetExamType { get; set; } // "ЕНТ", "SAT", "IELTS", "Custom"
    
    /// <summary>
    /// Желаемый университет (если цель = University)
    /// </summary>
    [Display(Name = "Целевой университет")]
    public int? TargetUniversityId { get; set; }
    public University? TargetUniversity { get; set; }
    
    /// <summary>
    /// Карьерная цель / специальность
    /// </summary>
    [Display(Name = "Карьерная цель")]
    [StringLength(200)]
    public string? CareerGoal { get; set; } // "IT", "Medicine", "Engineering", etc.
    
    // ========== ГЕОГРАФИЧЕСКИЕ ПРЕДПОЧТЕНИЯ ==========
    
    /// <summary>
    /// Предпочитаемая страна для обучения
    /// </summary>
    [Display(Name = "Предпочитаемая страна")]
    [StringLength(100)]
    public string? PreferredCountry { get; set; } // "Kazakhstan", "USA", "UK", "Russia", etc.
    
    /// <summary>
    /// Предпочитаемый город для обучения
    /// </summary>
    [Display(Name = "Предпочитаемый город")]
    [StringLength(100)]
    public string? PreferredCity { get; set; } // "Almaty", "Astana", "Moscow", etc.
    
    /// <summary>
    /// Готовность к переезду
    /// </summary>
    [Display(Name = "Готовность к переезду")]
    public bool WillingToRelocate { get; set; } = false;
    
    // ========== ФИНАНСОВЫЕ ПРЕДПОЧТЕНИЯ ==========
    
    /// <summary>
    /// Максимальный бюджет на обучение (год)
    /// </summary>
    [Display(Name = "Максимальный бюджет (год)")]
    [Range(0, double.MaxValue)]
    public decimal? MaxBudgetPerYear { get; set; }
    
    /// <summary>
    /// Интересуют ли гранты/стипендии
    /// </summary>
    [Display(Name = "Интересуют гранты")]
    public bool InterestedInScholarships { get; set; } = true;
    
    // ========== ЯЗЫКОВЫЕ ПРЕДПОЧТЕНИЯ ==========
    
    /// <summary>
    /// Предпочитаемый язык обучения (JSON: ["Russian", "English", "Kazakh"])
    /// </summary>
    [Display(Name = "Языки обучения")]
    [Column(TypeName = "jsonb")]
    public string PreferredLanguagesJson { get; set; } = "[\"Russian\"]";
    
    /// <summary>
    /// Уровень английского (A1, A2, B1, B2, C1, C2)
    /// </summary>
    [Display(Name = "Уровень английского")]
    [StringLength(10)]
    public string? EnglishLevel { get; set; }
    
    // ========== ПРЕДМЕТЫ ==========
    
    /// <summary>
    /// Предметы, которыми интересуется пользователь (Many-to-Many)
    /// </summary>
    [Display(Name = "Интересующие предметы")]
    public ICollection<Subject> InterestedSubjects { get; set; } = new List<Subject>();
    
    /// <summary>
    /// Предметы, в которых пользователь силен
    /// </summary>
    [Display(Name = "Сильные предметы")]
    public ICollection<Subject> StrongSubjects { get; set; } = new List<Subject>();
    
    /// <summary>
    /// Предметы, которые нужно подтянуть
    /// </summary>
    [Display(Name = "Слабые предметы")]
    public ICollection<Subject> WeakSubjects { get; set; } = new List<Subject>();
    
    // ========== ФОРМАТ ОБУЧЕНИЯ ==========
    
    /// <summary>
    /// Предпочитает ли карточки
    /// </summary>
    [Display(Name = "Предпочитает карточки")]
    public bool PrefersFlashcards { get; set; } = true;
    
    /// <summary>
    /// Предпочитает ли квизы
    /// </summary>
    [Display(Name = "Предпочитает квизы")]
    public bool PrefersQuizzes { get; set; } = true;
    
    /// <summary>
    /// Предпочитает ли экзамены
    /// </summary>
    [Display(Name = "Предпочитает экзамены")]
    public bool PrefersExams { get; set; } = false;
    
    /// <summary>
    /// Предпочитаемый уровень сложности (1=Easy, 2=Medium, 3=Hard)
    /// </summary>
    [Display(Name = "Предпочитаемая сложность")]
    [Range(1, 3)]
    public int PreferredDifficulty { get; set; } = 2; // Medium по умолчанию
    
    /// <summary>
    /// Планируемое время обучения в день (минуты)
    /// </summary>
    [Display(Name = "Ежедневное время обучения (мин)")]
    [Range(5, 480)] // от 5 минут до 8 часов
    public int DailyStudyTimeMinutes { get; set; } = 30;
    
    // ========== РАСПИСАНИЕ ==========
    
    /// <summary>
    /// Предпочитаемое время для обучения
    /// </summary>
    [Display(Name = "Предпочитаемое время")]
    [StringLength(20)]
    public string? PreferredStudyTime { get; set; } // "Morning", "Afternoon", "Evening", "Night"
    
    /// <summary>
    /// Дни недели для обучения (JSON: ["Mon", "Tue", "Wed"])
    /// </summary>
    [Display(Name = "Дни обучения")]
    [Column(TypeName = "jsonb")]
    public string StudyDaysJson { get; set; } = "[\"Mon\",\"Tue\",\"Wed\",\"Thu\",\"Fri\"]";
    
    // ========== МОТИВАЦИЯ ==========
    
    /// <summary>
    /// Уровень мотивации (1-5)
    /// </summary>
    [Display(Name = "Уровень мотивации")]
    [Range(1, 5)]
    public int MotivationLevel { get; set; } = 3;
    
    /// <summary>
    /// Нужны ли напоминания
    /// </summary>
    [Display(Name = "Включить напоминания")]
    public bool NeedsReminders { get; set; } = true;
    
    /// <summary>
    /// Завершен ли Onboarding опрос
    /// </summary>
    [Display(Name = "Onboarding завершен")]
    public bool OnboardingCompleted { get; set; } = false;
    
    // ========== МЕТАДАННЫЕ ==========
    
    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Дата обновления")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
