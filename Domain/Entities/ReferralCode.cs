namespace UniStart.Domain.Entities;

public class ReferralCode
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public virtual User Owner { get; set; } = null!;
    public string Code { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public int UsedCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
