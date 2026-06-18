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

    // Contact phone (Kazakhstan format, e.g. +77001234567). Nullable for legacy users;
    // they are forced to fill it in via the profile-completion gate on next login.
    public string? PhoneNumber { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public bool FreeMockUsed { get; set; } = false;

    // Tutor binding (student's linked tutor)
    public int? LinkedTutorId { get; set; }

    // School binding (White Label — auto-set on subdomain registration)
    public int? SchoolId { get; set; }

    // Email verification
    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationCodeExpiresAt { get; set; }

    // Password reset
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiresAt { get; set; }

    // External login (Google OAuth)
    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Online presence (T-9)
    public DateTime? LastSeenAt { get; set; }

    // Soft Delete (OP-9)
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // Block / Suspend (OP-14)
    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }
    public string? BlockReason { get; set; }

    // Account lockout (S-5)
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // Referral program
    public int? ReferredByCodeId { get; set; }
    public virtual ReferralCode? ReferredByCode { get; set; }

    // Navigation properties
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    public virtual ICollection<UserSkillProfile> SkillProfiles { get; set; } = new List<UserSkillProfile>();
    public virtual ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
    public virtual NotificationPreferences? NotificationPreferences { get; set; }
    public virtual TutorProfile? TutorProfile { get; set; }
    public virtual TutorSchool? School { get; set; }

    public bool IsPro => SubscriptionTier == SubscriptionTier.Pro
                         && (SubscriptionExpiresAt == null || SubscriptionExpiresAt > DateTime.UtcNow);
}

public enum UserRole
{
    Student,
    Tutor,
    Admin,
    SchoolAdmin,
    SchoolTutor
}

public enum SubscriptionTier
{
    Free,
    Pro
}
