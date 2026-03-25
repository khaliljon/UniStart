namespace UniStart.Domain.Entities;

public enum SchoolApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public class SchoolApplication
{
    public int Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? Message { get; set; }
    public SchoolApplicationStatus Status { get; set; } = SchoolApplicationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
}
