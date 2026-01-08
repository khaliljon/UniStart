using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniStart.Models.Core;
using UniStart.Models.Reference;

namespace UniStart.Models.Learning;

/// <summary>
/// Рекомендация университета для пользователя (результат работы ML/алгоритма)
/// </summary>
public class UniversityRecommendation
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// ID пользователя, для которого сделана рекомендация
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Навигационное свойство к пользователю
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
    
    /// <summary>
    /// ID университета
    /// </summary>
    [Required]
    public int UniversityId { get; set; }
    
    /// <summary>
    /// Навигационное свойство к университету
    /// </summary>
    public University University { get; set; } = null!;
    
    /// <summary>
    /// Оценка соответствия (0-100)
    /// Насколько университет подходит пользователю
    /// </summary>
    [Display(Name = "Оценка соответствия")]
    [Range(0, 100)]
    public double MatchScore { get; set; }
    
    /// <summary>
    /// Вероятность поступления (0-100)
    /// На основе баллов пользователя и требований университета
    /// </summary>
    [Display(Name = "Вероятность поступления")]
    [Range(0, 100)]
    public double AdmissionProbability { get; set; }
    
    /// <summary>
    /// Причины рекомендации (JSON массив строк)
    /// Например: ["Сильные результаты по математике", "Соответствует бюджету"]
    /// </summary>
    [Display(Name = "Причины рекомендации")]
    [Column(TypeName = "jsonb")]
    public string ReasonsJson { get; set; } = "[]";
    
    /// <summary>
    /// Ранг рекомендации (1 = лучшая, 2 = вторая и т.д.)
    /// </summary>
    [Display(Name = "Ранг")]
    [Range(1, int.MaxValue)]
    public int Rank { get; set; }
    
    /// <summary>
    /// Дата создания рекомендации
    /// </summary>
    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Была ли рекомендация просмотрена пользователем
    /// </summary>
    [Display(Name = "Просмотрена")]
    public bool IsViewed { get; set; } = false;
    
    /// <summary>
    /// Дата просмотра
    /// </summary>
    [Display(Name = "Дата просмотра")]
    public DateTime? ViewedAt { get; set; }
    
    /// <summary>
    /// Обратная связь от пользователя (1-5 звезд, null если не оценена)
    /// </summary>
    [Display(Name = "Оценка пользователя")]
    [Range(1, 5)]
    public int? UserRating { get; set; }
}
