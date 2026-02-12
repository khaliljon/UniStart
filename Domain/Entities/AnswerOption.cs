namespace UniStart.Domain.Entities;

public class AnswerOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Navigation properties
    public virtual Question Question { get; set; } = null!;
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
