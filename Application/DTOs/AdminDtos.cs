namespace UniStart.Application.DTOs;

// ─── Admin Question Management ──────────────────────────

public record QuestionListDto(
    int Id,
    string TopicName,
    string SectionName,
    string ExamTypeCode,
    string Text,
    string Difficulty,
    double DifficultyParam,
    double DiscriminationParam,
    int AnswerCount,
    DateTime CreatedAt
);

public record QuestionDetailDto(
    int Id,
    int TopicId,
    string TopicName,
    string SectionName,
    string ExamTypeCode,
    string Text,
    string Difficulty,
    string? Explanation,
    string? ImageUrl,
    double DifficultyParam,
    double DiscriminationParam,
    double GuessParam,
    DateTime CreatedAt,
    List<AdminAnswerOptionDto> AnswerOptions
);

public record AdminAnswerOptionDto(
    int Id,
    string Text,
    bool IsCorrect
);

public record CreateQuestionDto(
    int TopicId,
    string Text,
    string Difficulty,  // "Easy", "Medium", "Hard"
    string? Explanation,
    string? ImageUrl,
    double? DifficultyParam,
    double? DiscriminationParam,
    double? GuessParam,
    List<CreateAnswerOptionDto> AnswerOptions
);

public record CreateAnswerOptionDto(
    string Text,
    bool IsCorrect
);

public record UpdateQuestionDto(
    int? TopicId,
    string? Text,
    string? Difficulty,
    string? Explanation,
    string? ImageUrl,
    double? DifficultyParam,
    double? DiscriminationParam,
    double? GuessParam,
    List<CreateAnswerOptionDto>? AnswerOptions
);

public record BulkImportDto(
    List<CreateQuestionDto> Questions
);

public record BulkImportResultDto(
    int Total,
    int Imported,
    int Failed,
    List<string> Errors
);

public record QuestionStatsDto(
    int TotalQuestions,
    Dictionary<string, int> ByExam,
    Dictionary<string, int> ByDifficulty,
    Dictionary<string, int> ByTopic,
    int TopicsWithQuestions,
    int TopicsWithoutQuestions,
    List<string> TopicsWithoutQuestionsList
);

// ─── Admin User Management ──────────────────────────

public record AdminUserDto(
    int Id,
    string Email,
    string Name,
    string Role,
    string SubscriptionTier,
    DateTime? SubscriptionExpiresAt,
    bool HasCompletedOnboarding,
    bool IsBlocked,
    DateTime? BlockedAt,
    string? BlockReason,
    bool IsDeleted,
    DateTime? DeletedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int TotalAnswers,
    int CorrectAnswers,
    int TestSessions,
    int? SchoolId = null,
    string? SchoolName = null
);

public record AdminUpdateUserDto(
    string? Name = null,
    string? Email = null,
    string? Role = null,  // "Student", "Tutor", "Admin", "SchoolAdmin"
    string? SubscriptionTier = null,  // "Free", "Pro"
    DateTime? SubscriptionExpiresAt = null,
    int? SchoolId = null,  // bind user to a specific school
    bool ClearSchool = false  // explicitly unbind from school
);

public record BlockUserDto(
    string? Reason
);

// ─── Trash / Recycle Bin ────────────────────────────

public record TrashItemDto(
    int Id,
    string EntityType,     // "User" | "Question"
    string DisplayName,
    string? Detail,
    DateTime? DeletedAt,
    string? DeletedBy,
    int DaysUntilPurge
);

public record TrashSummaryDto(
    int TotalUsers,
    int TotalQuestions,
    List<TrashItemDto> Items
);

public record AdminUserStatsDto(
    int TotalUsers,
    int Students,
    int Tutors,
    int Admins,
    int ProUsers,
    int ActiveLast7Days
);

// ─── Admin Dashboard / Overview ─────────────────────

public record AdminDashboardDto(
    QuestionStatsDto QuestionStats,
    AdminUserStatsDto UserStats,
    List<AdminTopicSummaryDto> Topics
);

public record AdminTopicSummaryDto(
    int Id,
    string Name,
    string SectionName,
    string ExamTypeCode,
    int QuestionCount
);

public record CreateTopicDto(
    string Name,
    int SectionId,
    int SkillId
);

public record AdminSectionDto(
    int Id,
    string Name,
    string ExamTypeCode
);

public record UpdateTopicDto(
    string? Name
);

public record CreateSectionDto(
    string Name,
    string ExamTypeCode
);

public record UpdateSectionDto(
    string? Name
);

public record AdminSkillDto(
    int Id,
    string Code,
    string Name
);

// ─── Audit Log (OP-7) ──────────────────────────────────

public record AuditLogDto(
    long Id,
    int UserId,
    string UserEmail,
    string Action,
    string EntityType,
    string? EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTime Timestamp
);

public record AuditLogPagedResult(
    List<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ─── Generic Paged Result (OP-13) ────────────────

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
