using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Тема обучения - конкретная тема внутри компетенции (например, "Линейные уравнения", "Квадратные уравнения")
/// </summary>
public class LearningTopic
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
    public int LearningCompetencyId { get; set; }
    public LearningCompetency LearningCompetency { get; set; } = null!;
    
    // Теория - интерактивный контент
    public TheoryContent? Theory { get; set; }
    
    // Практический квиз - с объяснениями для обучения
    public int? PracticeQuizId { get; set; }
    public Quiz? PracticeQuiz { get; set; }
}

