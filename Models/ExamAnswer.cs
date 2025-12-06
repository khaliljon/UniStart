using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Вариант ответа на вопрос экзамена
/// </summary>
public class ExamAnswer
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Текст ответа обязателен")]
    [StringLength(500, ErrorMessage = "Текст ответа не должен превышать 500 символов")]
    public string Text { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
    
    // Связи
    public int QuestionId { get; set; }
    public ExamQuestion Question { get; set; } = null!;
}
