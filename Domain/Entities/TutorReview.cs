namespace UniStart.Domain.Entities;

public class TutorReview
{
    public int Id { get; set; }
    public int TutorProfileId { get; set; }
    public int StudentId { get; set; }
    public int Rating { get; set; }           // 1–5
    public string? Comment { get; set; }       // ≤1000
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual TutorProfile TutorProfile { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
}
