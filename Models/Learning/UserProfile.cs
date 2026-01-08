using System.ComponentModel.DataAnnotations;

namespace UniStart.Models.Learning;

/// <summary>
/// Агрегированный профиль пользователя для рекомендательной системы
/// (не хранится в БД, собирается динамически)
/// </summary>
public class UserProfile
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Баллы по предметам (Subject -> Average Score)
    /// Например: {"Math": 85.5, "Physics": 78.2, "Chemistry": 90.1}
    /// </summary>
    public Dictionary<string, double> SubjectScores { get; set; } = new();
    
    /// <summary>
    /// Предпочтительный город для обучения
    /// </summary>
    [StringLength(100)]
    public string? PreferredCity { get; set; }
    
    /// <summary>
    /// Предпочтительная страна для обучения
    /// </summary>
    [StringLength(100)]
    public string? PreferredCountry { get; set; }
    
    /// <summary>
    /// Максимальный бюджет на обучение (в год)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxBudget { get; set; }
    
    /// <summary>
    /// Карьерная цель / желаемая специальность
    /// Например: "IT", "Medicine", "Engineering"
    /// </summary>
    [StringLength(200)]
    public string? CareerGoal { get; set; }
    
    /// <summary>
    /// Средний балл по всем квизам и экзаменам
    /// </summary>
    [Range(0, 100)]
    public double AverageExamScore { get; set; }
    
    /// <summary>
    /// Количество пройденных квизов
    /// </summary>
    public int TotalQuizzesTaken { get; set; }
    
    /// <summary>
    /// Количество пройденных экзаменов
    /// </summary>
    public int TotalExamsTaken { get; set; }
    
    /// <summary>
    /// Сильные стороны (топ-3 предмета по баллам)
    /// </summary>
    public List<string> Strengths { get; set; } = new();
    
    /// <summary>
    /// Слабые стороны (нижние 3 предмета по баллам)
    /// </summary>
    public List<string> Weaknesses { get; set; } = new();
    
    /// <summary>
    /// Прогресс обучения (процент освоенных карточек)
    /// </summary>
    [Range(0, 100)]
    public double LearningProgress { get; set; }
}
