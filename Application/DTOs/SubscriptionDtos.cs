namespace UniStart.Application.DTOs;

public record SubscriptionStatusDto(
    string Tier,
    bool IsPro,
    DateTime? ExpiresAt,
    DailyUsageDto DailyUsage,
    TierLimitsDto Limits
);

public record DailyUsageDto(
    int QuestionsAnswered,
    int QuestionsLimit,
    int QuestionsRemaining,
    int LessonsViewed,
    int LessonsLimit,
    int LessonsRemaining,
    bool IsLimitReached
);

public record TierLimitsDto(
    int QuestionsPerDay,
    int LessonsPerDay,
    bool MockExamsEnabled,
    bool FullAnalytics,
    bool FullStudyPlan,
    bool RealtimePrediction
);

public record UpgradeRequestDto(
    string Plan
);

public record UpgradeResponseDto(
    bool Success,
    string Tier,
    DateTime? ExpiresAt,
    string Message
);
