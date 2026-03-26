namespace UniStart.Domain.Entities;

public enum TutorSchoolApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public class TutorSchoolApplication
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SchoolId { get; set; }
    public string? Message { get; set; }
    public TutorSchoolApplicationStatus Status { get; set; } = TutorSchoolApplicationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual TutorSchool School { get; set; } = null!;
}
