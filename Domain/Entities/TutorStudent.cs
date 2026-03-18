namespace UniStart.Domain.Entities;

/// <summary>
/// Привязка ученика к тьютору через инвайт-код.
/// Один ученик → один тьютор; тьютор → до MaxStudents учеников.
/// </summary>
public class TutorStudent
{
    public int Id { get; set; }
    public int TutorUserId { get; set; }
    public int StudentUserId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public TutorStudentStatus Status { get; set; } = TutorStudentStatus.Active;
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    // Navigation
    public virtual User TutorUser { get; set; } = null!;
    public virtual User StudentUser { get; set; } = null!;
}

public enum TutorStudentStatus
{
    Active,
    Revoked
}
