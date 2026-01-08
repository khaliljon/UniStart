using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniStart.Models.Core;

namespace UniStart.Models.Learning;

/// <summary>
/// Паттерны обучения пользователя (собираются и обновляются ML моделью)
/// </summary>
public class UserLearningPattern
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// ID пользователя
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Навигационное свойство к пользователю
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
    
    /// <summary>
    /// Средний процент удержания информации (retention rate)
    /// Рассчитывается на основе повторных тестов
    /// </summary>
    [Display(Name = "Средний процент удержания")]
    [Range(0, 100)]
    public double AverageRetentionRate { get; set; }
    
    /// <summary>
    /// Мастерство по предметам (JSON: {"Math": 85.5, "Physics": 72.3})
    /// </summary>
    [Display(Name = "Мастерство по предметам")]
    [Column(TypeName = "jsonb")]
    public string SubjectMasteryJson { get; set; } = "{}";
    
    /// <summary>
    /// Предпочтительное время для обучения (в часах от начала дня: 0-23)
    /// </summary>
    [Display(Name = "Предпочтительное время обучения (час)")]
    [Range(0, 23)]
    public int? PreferredStudyHour { get; set; }
    
    /// <summary>
    /// Средняя длительность сессии обучения (в минутах)
    /// </summary>
    [Display(Name = "Средняя длительность сессии (минуты)")]
    [Range(1, 300)]
    public int? AverageSessionMinutes { get; set; }
    
    /// <summary>
    /// Оптимальная сложность карточек для пользователя (0-1)
    /// </summary>
    [Display(Name = "Оптимальная сложность")]
    [Range(0, 1)]
    public double OptimalDifficulty { get; set; } = 0.5;
    
    /// <summary>
    /// Скорость забывания (faster = забывает быстрее, нужны более частые повторения)
    /// </summary>
    [Display(Name = "Скорость забывания")]
    [Range(0.1, 5.0)]
    public double ForgettingSpeed { get; set; } = 1.0;
    
    /// <summary>
    /// Дата последнего обновления паттерна
    /// </summary>
    [Display(Name = "Дата обновления")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Дата создания записи
    /// </summary>
    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Количество обработанных сессий для расчета паттернов
    /// </summary>
    [Display(Name = "Количество сессий")]
    public int SessionsProcessed { get; set; } = 0;
}
