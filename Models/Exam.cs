using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Модель экзамена - серьёзное образовательное тестирование с ограничениями и контролем
/// </summary>
public class Exam
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Название экзамена обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Предмет обязателен")]
    [StringLength(100)]
    [Obsolete("Используйте Subjects для множественного выбора")]
    public string Subject { get; set; } = string.Empty; // Оставлено для обратной совместимости
    
    [Required]
    [StringLength(50)]
    public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
    
    // Ограничения экзамена
    [Range(1, 10, ErrorMessage = "Количество попыток должно быть от 1 до 10")]
    public int MaxAttempts { get; set; } = 3; // Максимальное количество попыток
    
    [Range(0, 100, ErrorMessage = "Проходной балл должен быть от 0 до 100%")]
    public int PassingScore { get; set; } = 70; // Проходной балл в процентах
    
    public bool IsProctored { get; set; } = false; // Требуется ли контроль
    public bool ShuffleQuestions { get; set; } = true; // Перемешивать вопросы
    public bool ShuffleAnswers { get; set; } = true; // Перемешивать варианты ответов
    
    // Настройки показа результатов
    [Required]
    [StringLength(50)]
    public string ShowResultsAfter { get; set; } = "Immediate"; // Immediate, AfterDeadline, Manual
    
    public bool ShowCorrectAnswers { get; set; } = true; // Показывать правильные ответы
    public bool ShowDetailedFeedback { get; set; } = true; // Показывать детальную обратную связь
    
    // Временные ограничения
    [Range(1, 300, ErrorMessage = "Ограничение времени должно быть от 1 до 300 минут")]
    public int TimeLimit { get; set; } = 60; // Ограничение времени в минутах
    
    public bool StrictTiming { get; set; } = true; // Автозавершение при истечении времени
    
    // Метаданные
    public bool IsPublished { get; set; } = false;
    public bool IsPublic { get; set; } = false; // true = доступен всем студентам, false = только студентам автора
    public int TotalPoints { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Международная система
    [Display(Name = "Страна")]
    public int? CountryId { get; set; }
    public Country? Country { get; set; }
    
    [Display(Name = "Университет")]
    public int? UniversityId { get; set; }
    public University? University { get; set; }
    
    [Display(Name = "Тип экзамена")]
    public int? ExamTypeId { get; set; }
    public ExamType? ExamType { get; set; }
    
    // Связи
    public string UserId { get; set; } = string.Empty; // Создатель экзамена
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
    public ICollection<UserExamAttempt> Attempts { get; set; } = new List<UserExamAttempt>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>(); // Множественный выбор предметов
}
