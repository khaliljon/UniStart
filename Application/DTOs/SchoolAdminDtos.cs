namespace UniStart.Application.DTOs;

// ═══════════════════════════════════════════════════════
//  SCHOOL ADMIN DASHBOARD DTOs
// ═══════════════════════════════════════════════════════

public record SchoolDashboardDto(
    int SchoolId,
    string SchoolName,
    int TotalStudents,
    int TotalTutors,
    int ActiveStudentsLast7Days,
    double AverageAccuracy,
    List<SchoolStudentDto> RecentStudents
);

public record SchoolStudentDto(
    int UserId,
    string Name,
    string Email,
    string SubscriptionTier,
    DateTime CreatedAt,
    DateTime? LastSeenAt,
    int? LinkedTutorId,
    string? LinkedTutorName
);

public record SchoolTutorDto(
    int UserId,
    string Name,
    string Email,
    string Headline,
    string[] Specializations,
    bool IsVerified,
    bool IsAvailable,
    int TotalStudents,
    decimal AverageRating,
    DateTime CreatedAt
);
