namespace UniStart.Domain.Entities;

public class User : ISoftDeletable, IAuditable
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public bool HasCompletedOnboarding { get; set; } = false;

    public string? PhoneNumber { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public bool FreeMockUsed { get; set; } = false;

    public int? LinkedTutorId { get; set; }

    public int? SchoolId { get; set; }

    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationCodeExpiresAt { get; set; }

    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiresAt { get; set; }

    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }
    public string? BlockReason { get; set; }

    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public int? ReferredByCodeId { get; set; }
    public virtual ReferralCode? ReferredByCode { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    public virtual ICollection<UserSkillProfile> SkillProfiles { get; set; } = new List<UserSkillProfile>();
    public virtual ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();

    public bool IsPro => SubscriptionTier == SubscriptionTier.Pro
                         && (SubscriptionExpiresAt == null || SubscriptionExpiresAt > DateTime.UtcNow);
}

public enum UserRole
{
    Student = 0,
    Admin = 2
}

public enum SubscriptionTier
{
    Free,
    Pro
}
