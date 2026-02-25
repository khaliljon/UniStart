namespace UniStart.Domain.Entities;

public class NotificationPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // Email notification toggles
    public bool WelcomeEmail { get; set; } = true;
    public bool StreakReminder { get; set; } = true;
    public bool WeeklyDigest { get; set; } = true;
    public bool StudyPlanReminder { get; set; } = true;
    public bool AchievementNotification { get; set; } = true;

    // Tracking
    public DateTime? LastStreakReminderSentAt { get; set; }
    public DateTime? LastWeeklyDigestSentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
}
