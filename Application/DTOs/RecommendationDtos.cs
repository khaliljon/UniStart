namespace UniStart.Application.DTOs;


public record RecommendationDto(
    string Type,
    string Priority,
    string Title,
    string Description,
    string? Icon,
    string? ActionLabel,
    string? ActionUrl,
    Dictionary<string, object>? Metadata
);

public record DailyBriefingDto(
    int CurrentStreak,
    int LongestStreak,
    List<RecommendationDto> Recommendations,
    List<MilestoneDto> RecentMilestones,
    StreakDto Streak,
    DailySummaryDto? YesterdaySummary
);

public record StreakDto(
    int CurrentStreak,
    int LongestStreak,
    bool StudiedToday,
    DateTime? LastStudyDate,
    int TotalStudyDays
);

public record MilestoneDto(
    int Id,
    string Code,
    string Title,
    string Description,
    string Icon,
    DateTime AchievedAt,
    bool IsNew
);

public record DailySummaryDto(
    int QuestionsAnswered,
    int CorrectAnswers,
    double Accuracy,
    int MinutesSpent,
    int TopicsStudied
);

public record AfterSessionDto(
    List<RecommendationDto> Recommendations
);
