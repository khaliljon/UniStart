using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

public class Subject
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Связь с курсом (опционально, предмет может быть в курсе или standalone)
    public int? CourseId { get; set; }
    public Course? Course { get; set; }
    
    // Иерархия обучения
    public ICollection<LearningModule> LearningModules { get; set; } = new List<LearningModule>();
}
