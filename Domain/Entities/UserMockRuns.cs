namespace UniStart.Domain.Entities;

/// <summary>
/// A user's remaining run balance for a specific mock template.
/// Incremented when a purchase is granted (stub checkout or Polar webhook),
/// decremented each time the user starts a new session.
/// </summary>
public class UserMockRuns : IAuditable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MockExamId { get; set; }

    public int RunsRemaining { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual MockExam MockExam { get; set; } = null!;
}
