namespace UniStart.Models;

/// <summary>
/// Ответ студента на вопрос теста
/// </summary>
public class UserTestAnswer
{
    public int Id { get; set; }
    
    public bool IsCorrect { get; set; } // Правильно ли ответил
    public int PointsEarned { get; set; } // Заработанные баллы за этот вопрос
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    
    // Связи
    public int AttemptId { get; set; }
    public UserTestAttempt Attempt { get; set; } = null!;
    public int QuestionId { get; set; }
    public TestQuestion Question { get; set; } = null!;
    public int SelectedAnswerId { get; set; }
    public TestAnswer SelectedAnswer { get; set; } = null!;
}
