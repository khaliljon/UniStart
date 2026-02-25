namespace UniStart.Application.DTOs;

// ─── Notification Preferences ───────────────────────────

public record NotificationPreferencesDto(
    bool WelcomeEmail,
    bool StreakReminder,
    bool WeeklyDigest,
    bool StudyPlanReminder,
    bool AchievementNotification
);

public record UpdateNotificationPreferencesDto(
    bool? WelcomeEmail,
    bool? StreakReminder,
    bool? WeeklyDigest,
    bool? StudyPlanReminder,
    bool? AchievementNotification
);

// ─── Email Log ──────────────────────────────────────────

public record EmailLogDto(
    string Type,
    string Subject,
    DateTime SentAt,
    bool Success
);

// ─── Weekly Digest Content ──────────────────────────────

public record WeeklyDigestDataDto(
    string UserName,
    int QuestionsAnswered,
    int CorrectAnswers,
    double Accuracy,
    int CurrentStreak,
    int PredictedScore,
    int MaxPossibleScore,
    string ExamName,
    List<WeeklyTopicProgressDto> TopProgress,
    List<string> Recommendations
);

public record WeeklyTopicProgressDto(
    string TopicName,
    int QuestionsAnswered,
    double Accuracy,
    string Trend // "improving", "stable", "declining"
);
