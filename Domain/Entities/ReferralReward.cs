namespace UniStart.Domain.Entities;

public class ReferralReward
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public virtual User Owner { get; set; } = null!;
    public int ReferralUsageId { get; set; }
    public virtual ReferralUsage Usage { get; set; } = null!;
    public string RewardType { get; set; } = "";   // "money" or "days"
    public decimal Amount { get; set; }              // 500 (₸) or 5 (days)
    public bool IsPaidOut { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidOutAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
