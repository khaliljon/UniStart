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
    DateTime LastUpdated
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
