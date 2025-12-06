using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Вопрос экзамена
/// </summary>
public class ExamQuestion
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Текст вопроса обязателен")]
    [StringLength(1000, ErrorMessage = "Текст вопроса не должен превышать 1000 символов")]
    public string Text { get; set; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "Объяснение не должно превышать 2000 символов")]
    public string? Explanation { get; set; } // Объяснение правильного ответа
    
    [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
    public int Points { get; set; } = 1;
    
    public int Order { get; set; }
    
    // Связи
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public ICollection<ExamAnswer> Answers { get; set; } = new List<ExamAnswer>();
}
