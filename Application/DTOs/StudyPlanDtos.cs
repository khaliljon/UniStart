namespace UniStart.Application.DTOs;

// ─── Requests ────────────────────────────────────────────────

public record CreateStudyGoalDto(
    string ExamTypeCode,
    DateTime TargetDate,
    int TargetScore
);

public record UpdateStudyGoalDto(
    DateTime? TargetDate = null,
    int? TargetScore = null,
    bool? IsActive = null
);

public record CompleteEntryDto(
    int QuestionsAnswered = 0,
    int CorrectAnswers = 0
);

// ─── Responses ───────────────────────────────────────────────

public record StudyGoalDto(
    int Id,
    string ExamTypeCode,
    string ExamTypeName,
    DateTime TargetDate,
    int TargetScore,
    bool IsActive,
    int DaysUntilExam,
    double RecommendedHoursPerDay,
    DateTime CreatedAt
);

public record StudyPlanDto(
    int Id,
    int GoalId,
    string ExamTypeCode,
    string ExamTypeName,
    DateTime GeneratedAt,
    bool IsActive,
    int TotalEntries,
    int CompletedEntries,
    double CompletionPercent,
    IEnumerable<StudyPlanEntryDto> Entries
);

public record StudyPlanEntryDto(
    int Id,
    int TopicId,
    string TopicName,
    DateTime Date,
    int RecommendedMinutes,
    string Type,
    int RecommendedQuestions,
    bool IsCompleted,
    DateTime? CompletedAt,
    int QuestionsAnswered,
    int CorrectAnswers
);

public record TodayPlanDto(
    DateTime Date,
    bool HasGoal,
    string? ExamTypeCode,
    string? ExamTypeName,
    int DaysUntilExam,
    int TotalMinutesToday,
    IEnumerable<StudyPlanEntryDto> Entries,
    string Recommendation
);

public record PlanStatsDto(
    int TotalDays,
    int CompletedDays,
    int SkippedDays,
    double AverageAccuracy,
    int TotalQuestionsAnswered,
    double AdherencePercent,
    IEnumerable<WeekSummaryDto> WeeklySummary
);

public record WeekSummaryDto(
    DateTime WeekStart,
    int PlannedEntries,
    int CompletedEntries,
    int TotalMinutesPlanned,
    double Accuracy
);
