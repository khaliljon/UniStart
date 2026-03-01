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
    List<ReviewDto> RecentReviews
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
    string Status
);

public record MessageDto(
    long Id,
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
    int TutorId
);

public record MessagesPageDto(
    List<MessageDto> Items,
    int TotalCount,
    bool HasMore
);
