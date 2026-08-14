namespace UniStart.Domain.Entities;

public class MockExamAnswerOption
{
    public int MockExamAnswerId { get; set; }
    public int AnswerOptionId { get; set; }

    public virtual MockExamAnswer Answer { get; set; } = null!;
    public virtual AnswerOption Option { get; set; } = null!;
}
