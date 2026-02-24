namespace UniStart.Application.DTOs;

// Skill & Analytics DTOs
public record SkillDto(
    int Id,
    string Code,
    string Name,
    string? Description
);

public record UserSkillProfileDto(
    int SkillId,
    string SkillName,
    string SkillCode,
    int Level,
    DateTime LastUpdated,
    double Theta = 0.0,
    double ThetaSE = 1.0,
    int ConfidenceLow = 0,
    int ConfidenceHigh = 100
);

public record SkillAnalyticsDto(
    int TotalQuestionsAnswered,
    int CorrectAnswers,
    double OverallAccuracy,
    IEnumerable<UserSkillProfileDto> SkillProfiles,
    IEnumerable<SkillProgressDto> RecentProgress
);

public record SkillProgressDto(
    string SkillName,
    int OldLevel,
    int NewLevel,
    DateTime Date
);

public record TopicDto(
    int Id,
    string Name,
    int SkillId,
    string SkillName,
    int? SectionId,
    string? SectionName
);

// Enhanced analytics DTOs for Stage 4

/// <summary>
/// Full dashboard data returned by GET /api/analytics/dashboard
/// </summary>
public record DashboardDto(
    int TotalQuestionsAnswered,
    int CorrectAnswers,
    double OverallAccuracy,
    int CurrentStreak,
    int BestStreak,
    IEnumerable<UserSkillProfileDto> SkillProfiles,
    IEnumerable<SkillHistoryPointDto> SkillHistory,
    IEnumerable<DailyActivityDto> ActivityHeatmap,
    IEnumerable<DifficultyStatsDto> DifficultyBreakdown
);

/// <summary>
/// A single skill level data point over time (for line chart)
/// </summary>
public record SkillHistoryPointDto(
    string SkillName,
    string SkillCode,
    int Level,
    DateTime Date
);

/// <summary>
/// Daily activity for heatmap (GitHub-style contributions)
/// </summary>
public record DailyActivityDto(
    DateTime Date,
    int QuestionsAnswered,
    int CorrectCount
);

/// <summary>
/// Accuracy broken down by difficulty level
/// </summary>
public record DifficultyStatsDto(
    string Difficulty,
    int TotalAnswered,
    int CorrectCount,
    double Accuracy
);

/// <summary>
/// Test session summary for session history list
/// </summary>
public record TestSessionSummaryDto(
    int Id,
    string ExamTypeCode,
    string ExamTypeName,
    string Mode,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int TotalQuestions,
    int CorrectCount,
    double? Score
);

/// <summary>
/// Detailed test session with all answers
/// </summary>
public record TestSessionDetailDto(
    int Id,
    string ExamTypeCode,
    string ExamTypeName,
    string Mode,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int TotalQuestions,
    int CorrectCount,
    double? Score,
    IEnumerable<SessionAnswerDto> Answers
);

/// <summary>
/// Individual answer within a test session
/// </summary>
public record SessionAnswerDto(
    int QuestionId,
    string QuestionText,
    string TopicName,
    string Difficulty,
    int SelectedOptionId,
    string SelectedOptionText,
    int CorrectOptionId,
    string CorrectOptionText,
    bool IsCorrect,
    string? Explanation,
    int? TimeSpentSeconds
);
