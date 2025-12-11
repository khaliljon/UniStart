using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Ответ пользователя на вопрос квиза (аналогично UserExamAnswer для единообразия)
/// </summary>
public class UserQuizAnswer
{
    [Display(Name = "Идентификатор")]
    public int Id { get; set; }
    
    [Display(Name = "Правильный ответ")]
    public bool IsCorrect { get; set; } // Правильно ли ответил
    
    [Display(Name = "Заработанные баллы")]
    public int PointsEarned { get; set; } // Заработанные баллы за этот вопрос
    
    [Display(Name = "Время ответа")]
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    
    // Связи
    [Display(Name = "ID попытки")]
    public int AttemptId { get; set; }
    
    [Display(Name = "Попытка")]
    public UserQuizAttempt Attempt { get; set; } = null!;
    
    [Display(Name = "ID вопроса")]
    public int QuestionId { get; set; }
    
    [Display(Name = "Вопрос")]
    public QuizQuestion Question { get; set; } = null!;
    
    [Display(Name = "ID выбранного ответа")]
    public int? SelectedAnswerId { get; set; } // Может быть null для текстовых вопросов
    
    [Display(Name = "Выбранный ответ")]
    public QuizAnswer? SelectedAnswer { get; set; }
    
    // Для множественного выбора может быть несколько записей UserQuizAnswer с одним QuestionId
    // но разными SelectedAnswerId
}

