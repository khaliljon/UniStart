using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Exams;

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
