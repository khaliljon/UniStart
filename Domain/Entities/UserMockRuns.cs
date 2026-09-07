namespace UniStart.Domain.Entities;

public class UserMockRuns : IAuditable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MockExamId { get; set; }

    public string Language { get; set; } = "en";

    public int RunsRemaining { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual MockExam MockExam { get; set; } = null!;
}
