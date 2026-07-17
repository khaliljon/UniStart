namespace UniStart.Domain.Entities;

/// <summary>
/// Join row linking a <see cref="MockExamAnswer"/> to a selected <see cref="AnswerOption"/>.
/// Used for multiple-choice questions where more than one option can be selected.
/// </summary>
public class MockExamAnswerOption
{
    public int MockExamAnswerId { get; set; }
    public int AnswerOptionId { get; set; }

    public virtual MockExamAnswer Answer { get; set; } = null!;
    public virtual AnswerOption Option { get; set; } = null!;
}
