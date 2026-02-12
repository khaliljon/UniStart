namespace UniStart.Domain.Entities;

public class UserAnswer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QuestionId { get; set; }
    public int AnswerOptionId { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
    public virtual AnswerOption AnswerOption { get; set; } = null!;
}
