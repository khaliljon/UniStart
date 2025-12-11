using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Компетенция - конкретная компетенция внутри модуля (например, "Решение уравнений", "Работа с функциями")
/// </summary>
public class LearningCompetency
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public string? Icon { get; set; } // Emoji или иконка
    
    public int OrderIndex { get; set; } // Порядок отображения
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Связи
    public int LearningModuleId { get; set; }
    public LearningModule LearningModule { get; set; } = null!;
    
    public ICollection<LearningTopic> Topics { get; set; } = new List<LearningTopic>();
}

