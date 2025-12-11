using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Теоретический контент - интерактивная теория для темы обучения
/// Может содержать текст, изображения, формулы, видео, интерактивные элементы
/// </summary>
public class TheoryContent
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    // Содержимое в формате Markdown или HTML
    [Required]
    public string Content { get; set; } = string.Empty;
    
    // Метаданные для отображения
    public string? CoverImageUrl { get; set; }
    
    public int EstimatedReadTimeMinutes { get; set; } // Примерное время чтения в минутах
    
    // Дополнительные материалы (ссылки на видео, PDF и т.д.)
    [MaxLength(1000)]
    public string? AdditionalResources { get; set; } // JSON массив ссылок
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Связь с темой (один к одному)
    public int LearningTopicId { get; set; }
    public LearningTopic LearningTopic { get; set; } = null!;
}

