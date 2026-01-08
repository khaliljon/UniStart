using System.ComponentModel.DataAnnotations;

namespace UniStart.Models.Learning;

/// <summary>
/// Результат ML-предсказания оптимального времени повторения карточки
/// </summary>
public class FlashcardReviewPrediction
{
    /// <summary>
    /// Рекомендуемое время до следующего повторения (в часах)
    /// </summary>
    [Display(Name = "Оптимальное время повторения (часы)")]
    [Range(1, 8760, ErrorMessage = "Время должно быть от 1 часа до года")]
    public int OptimalReviewHours { get; set; }
    
    /// <summary>
    /// Уверенность модели в предсказании (0-1)
    /// </summary>
    [Display(Name = "Уверенность модели")]
    [Range(0, 1, ErrorMessage = "Уверенность должна быть от 0 до 1")]
    public float Confidence { get; set; }
    
    /// <summary>
    /// Объяснение рекомендации для пользователя
    /// </summary>
    [Display(Name = "Причина рекомендации")]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата и время следующего рекомендованного повторения
    /// </summary>
    [Display(Name = "Рекомендованная дата повторения")]
    public DateTime RecommendedReviewDate { get; set; }
    
    /// <summary>
    /// ID карточки, для которой сделано предсказание
    /// </summary>
    public int FlashcardId { get; set; }
    
    /// <summary>
    /// ID пользователя
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата создания предсказания
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
