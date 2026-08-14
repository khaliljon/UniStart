namespace UniStart.Domain.Entities;

public class NotificationPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public bool WelcomeEmail { get; set; } = true;
    public bool StreakReminder { get; set; } = true;
    public bool WeeklyDigest { get; set; } = true;
    public bool StudyPlanReminder { get; set; } = true;
    public bool AchievementNotification { get; set; } = true;

    public DateTime? LastStreakReminderSentAt { get; set; }
    public DateTime? LastWeeklyDigestSentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
