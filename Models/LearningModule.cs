using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Модуль обучения - тематическая единица внутри предмета (например, "Алгебра", "Геометрия")
/// Может содержать информацию о количестве вопросов в ЕНТ
/// </summary>
public class LearningModule
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public string? Icon { get; set; } // Emoji или иконка
    
    public int OrderIndex { get; set; } // Порядок отображения
    
    // Информация о ЕНТ
    public int? EntQuestionCount { get; set; } // Количество вопросов в ЕНТ (например, 15)
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Связи
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    
    public ICollection<LearningCompetency> Competencies { get; set; } = new List<LearningCompetency>();
    
    // Итоговый кейс модуля (анализ реальных данных)
    public int? CaseStudyQuizId { get; set; }
    public Quiz? CaseStudyQuiz { get; set; }
    
    // Финальный квиз модуля (без объяснений, проверка знаний всего модуля)
    public int? ModuleFinalQuizId { get; set; }
    public Quiz? ModuleFinalQuiz { get; set; }
}

