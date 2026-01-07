using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Learning;

/// <summary>
/// Курс обучения - верхний уровень иерархии (например, "ЕНТ 2025 - Физико-математическое направление")
/// </summary>
public class Course
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public string? Icon { get; set; } // Emoji или иконка
    
    public string? CoverImageUrl { get; set; } // Обложка курса
    
    public int? Year { get; set; } // Год курса (например, 2025)
    
    public string? Direction { get; set; } // Направление (например, "Физико-математическое")
    
    public int OrderIndex { get; set; } // Порядок отображения
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Связи
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}

