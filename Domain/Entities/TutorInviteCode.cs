namespace UniStart.Domain.Entities;

/// <summary>
/// Invite code with usage limits and expiration (S-6).
/// Replaces the simple InviteCode string on TutorProfile.
/// </summary>
public class TutorInviteCode
{
    public int Id { get; set; }
    public int TutorUserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? MaxUses { get; set; }     // null = unlimited
    public int UsedCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }     // "Для группы 11А"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User TutorUser { get; set; } = null!;
    public virtual ICollection<TutorInviteCodeUsage> Usages { get; set; } = new List<TutorInviteCodeUsage>();
}

public class TutorInviteCodeUsage
{
    public int Id { get; set; }
    public int InviteCodeId { get; set; }
    public int StudentUserId { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual TutorInviteCode InviteCode { get; set; } = null!;
    public virtual User StudentUser { get; set; } = null!;
}
