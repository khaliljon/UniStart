namespace UniStart.Application.DTOs;

// ═══════════════════════════════════════════════════════
//  TUTOR DTOs
// ═══════════════════════════════════════════════════════

public record TutorCardDto(
    int UserId,
    string Name,
    string Headline,
    string Bio,
    string[] Specializations,
    decimal AverageRating,
    int TotalReviews,
    int TotalStudents,
    bool IsAvailable,
    bool IsVerified,
    decimal? HourlyRate,
    string? AvatarUrl
);

public record TutorProfileDetailDto(
    int UserId,
    string Name,
    string Email,
    string Headline,
    string Bio,
    string Experience,
    string[] Specializations,
    decimal AverageRating,
    int TotalReviews,
    int TotalStudents,
    bool IsAvailable,
    bool IsVerified,
    decimal? HourlyRate,
    string? AvatarUrl,
    string ContactPreference,
    DateTime CreatedAt,
    List<ScheduleSlotDto> Schedule,
    List<ReviewDto> RecentReviews,
    DateTime? VerificationRequestedAt = null,
    bool HasPaidSubscription = false,
    int? SchoolId = null,
    string[]? TeachingSections = null,
    DateTime? SubscriptionExpiresAt = null,
    bool SchoolHasActiveSubscription = false
);

public record ScheduleSlotDto(
    int Id,
    int DayOfWeek,
    string DayName,
    string StartTime,
    string EndTime
);

public record ReviewDto(
    int Id,
    int StudentId,
    string StudentName,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);

public record UpdateTutorProfileDto(
    string? Headline,
    string? Bio,
    string? Experience,
    string[]? Specializations,
    string[]? TeachingSections,
    decimal? HourlyRate,
    bool? IsAvailable,
    string? ContactPreference
);

public record SetScheduleDto(
    List<ScheduleSlotInputDto> Slots
);

public record ScheduleSlotInputDto(
    int DayOfWeek,
    string StartTime,
    string EndTime
);

public record CreateReviewDto(
    int Rating,
    string? Comment
);

public record TutorListResultDto(
    List<TutorCardDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ═══════════════════════════════════════════════════════
//  MESSAGING DTOs
// ═══════════════════════════════════════════════════════

public record ConversationDto(
    int Id,
    int OtherUserId,
    string OtherUserName,
    string OtherUserRole,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    int UnreadCount,
    string Status,
    string? RequestMessage
);

public record MessageDto(
    long Id,
    int ConversationId,
    int SenderId,
    string SenderName,
    string Text,
    DateTime SentAt,
    DateTime? ReadAt,
    bool IsEdited,
    string Type,
    bool IsMine
);

public record SendMessageDto(
    string Text
);

public record StartConversationDto(
    int TutorId,
    string? Message   // Optional request message
);

public record StudentDto(
    int UserId,
    string Name,
    string Email,
    DateTime ConversationStartedAt,
    DateTime? LastMessageAt,
    string? LastMessagePreview
);

public record PendingRequestDto(
    int ConversationId,
    int StudentId,
    string StudentName,
    string StudentEmail,
    string? RequestMessage,
    DateTime RequestedAt
);

public record AcceptDeclineResultDto(
    int ConversationId,
    string Status,
    string? SystemMessage
);

public record DeclineRequestDto(
    string? Reason
);

public record MessagesPageDto(
    List<MessageDto> Items,
    int TotalCount,
    bool HasMore
);

// ─── Tutor School DTOs ───────────────────────────────────

public record SchoolBrandingDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? DescriptionEn,
    string? DescriptionKz,
    string? LogoUrl,
    string? PrimaryColor,
    string? PrimaryHoverColor,
    string? AccentColor,
    string? NavbarTitle,
    string[] Specializations
);

public record TutorSchoolCardDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    string? LogoUrl,
    string? InstagramUrl,
    string? TelegramUrl,
    string? WebsiteUrl,
    string[] Specializations,
    bool IsPartner,
    int TutorCount
);

public record TutorSchoolDetailDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    string? LogoUrl,
    string? InstagramUrl,
    string? TelegramUrl,
    string? WebsiteUrl,
    string[] Specializations,
    bool IsPartner,
    int? OwnerUserId,
    List<TutorCardDto> Tutors
);

// ─── School Management DTOs (Этап 4) ─────────────────────

public record CreateSchoolDto(
    string Name,
    string? Description,
    string? LogoUrl,
    string? WebsiteUrl,
    string? InstagramUrl,
    string? TelegramUrl,
    string? Specializations
);

public record UpdateSchoolDto(
    string? Name = null,
    string? Description = null,
    string? DescriptionEn = null,
    string? DescriptionKz = null,
    string? LogoUrl = null,
    string? WebsiteUrl = null,
    string? InstagramUrl = null,
    string? TelegramUrl = null,
    string? Specializations = null
);

public record SchoolAdminDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    string? DescriptionEn,
    string? DescriptionKz,
    string? LogoUrl,
    string? InstagramUrl,
    string? TelegramUrl,
    string? WebsiteUrl,
    string[] Specializations,
    bool IsPartner,
    bool IsActive,
    int? OwnerUserId,
    int TutorCount,
    DateTime CreatedAt
);

// ═══════════════════════════════════════════════════════
//  TUTOR-STUDENT BINDING DTOs
// ═══════════════════════════════════════════════════════

public record InviteCodeDto(
    string InviteCode
);

public record InviteCodeDetailDto(
    int Id,
    string Code,
    int? MaxUses,
    int UsedCount,
    DateTime? ExpiresAt,
    bool IsActive,
    string? Note,
    DateTime CreatedAt
);

public record CreateInviteCodeDto(
    int? MaxUses,
    DateTime? ExpiresAt,
    string? Note
);

public record LinkByInviteDto(
    string InviteCode
);

public record TutorStudentDto(
    int Id,
    int StudentUserId,
    string StudentName,
    string StudentEmail,
    string Status,
    DateTime LinkedAt,
    DateTime? RevokedAt
);

public record LinkedTutorDto(
    int TutorUserId,
    string TutorName,
    string Headline,
    string[] Specializations,
    decimal AverageRating,
    string? AvatarUrl,
    DateTime LinkedAt
);

public record LinkResultDto(
    bool Success,
    string Message
);

// ─── Tutor Question Management (Этап 2) ─────────────

public record TutorQuestionListItemDto(
    int Id,
    string Text,
    string Difficulty,
    string TopicName,
    string SectionName,
    string ExamTypeCode,
    int AnswerCount,
    DateTime CreatedAt
);

public record TutorQuestionDetailDto(
    int Id,
    int TopicId,
    string TopicName,
    string SectionName,
    string ExamTypeCode,
    string Text,
    string Difficulty,
    string? Explanation,
    DateTime CreatedAt,
    List<AdminAnswerOptionDto> AnswerOptions
);

public record TutorQuestionsPageDto(
    List<TutorQuestionListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ═══════════════════════════════════════════════════════
//  ASSIGNMENT DTOs (Sprint 7 Этап 3)
// ═══════════════════════════════════════════════════════

public record CreateAssignmentDto(
    string Title,
    string? Description,
    DateTime? Deadline,
    List<int> QuestionIds,
    List<int> StudentUserIds
);

public record UpdateAssignmentDto(
    string? Title,
    string? Description,
    DateTime? Deadline,
    bool? IsActive
);

public record AssignmentListItemDto(
    int Id,
    string Title,
    string? Description,
    DateTime? Deadline,
    bool IsActive,
    int QuestionCount,
    int StudentCount,
    int CompletedCount,
    DateTime CreatedAt
);

public record AssignmentDetailDto(
    int Id,
    string Title,
    string? Description,
    DateTime? Deadline,
    bool IsActive,
    DateTime CreatedAt,
    List<AssignmentQuestionDto> Questions,
    List<AssignmentStudentProgressDto> Students
);

public record AssignmentQuestionDto(
    int QuestionId,
    string Text,
    string Difficulty,
    string TopicName,
    int OrderIndex
);

public record AssignmentStudentProgressDto(
    int StudentUserId,
    string StudentName,
    string Status,
    int AnsweredCount,
    int CorrectCount,
    int TotalQuestions,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int? Score
);

// Student-facing DTOs
public record StudentAssignmentListItemDto(
    int Id,
    string Title,
    string? Description,
    DateTime? Deadline,
    string TutorName,
    string Status,
    int TotalQuestions,
    int AnsweredCount,
    int CorrectCount,
    int? Score,
    DateTime CreatedAt
);

public record StudentAssignmentDetailDto(
    int Id,
    string Title,
    string? Description,
    DateTime? Deadline,
    string TutorName,
    string Status,
    int TotalQuestions,
    int AnsweredCount,
    List<StudentAssignmentQuestionDto> Questions
);

public record StudentAssignmentQuestionDto(
    int QuestionId,
    string Text,
    string Difficulty,
    List<AdminAnswerOptionDto> Options,
    int? SelectedOptionId,
    bool? IsCorrect,
    string? Explanation
);

public record SubmitAssignmentAnswerDto(
    int QuestionId,
    int SelectedOptionId
);

public record SubmitAssignmentAnswerResultDto(
    bool IsCorrect,
    int CorrectOptionId,
    string? Explanation,
    int AnsweredCount,
    int TotalQuestions,
    bool IsCompleted,
    int? Score
);
