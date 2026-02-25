namespace UniStart.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public bool HasCompletedOnboarding { get; set; } = false;
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    public virtual ICollection<UserSkillProfile> SkillProfiles { get; set; } = new List<UserSkillProfile>();
    public virtual ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
    public virtual NotificationPreferences? NotificationPreferences { get; set; }

    public bool IsPro => SubscriptionTier == SubscriptionTier.Pro
                         && (SubscriptionExpiresAt == null || SubscriptionExpiresAt > DateTime.UtcNow);
}

public enum UserRole
{
    Student,
    Tutor,
    Admin
}

public enum SubscriptionTier
{
    Free,
    Pro
}
