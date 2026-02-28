namespace UniStart.Domain.Entities;

/// <summary>
/// Audit trail for all admin actions (OP-7).
/// Stores who did what, when, and the before/after state.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;          // "Create", "Update", "Delete"
    public string EntityType { get; set; } = string.Empty;      // "Question", "User", "Topic"
    public string? EntityId { get; set; }                        // PK of affected entity
    public string? OldValues { get; set; }                       // JSON snapshot before change
    public string? NewValues { get; set; }                       // JSON snapshot after change
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
}
