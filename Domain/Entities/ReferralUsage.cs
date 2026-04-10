namespace UniStart.Domain.Entities;

public class ReferralUsage
{
    public int Id { get; set; }
    public int ReferralCodeId { get; set; }
    public virtual ReferralCode ReferralCode { get; set; } = null!;
    public int ReferredUserId { get; set; }
    public virtual User ReferredUser { get; set; } = null!;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public bool RewardGranted { get; set; }
}
