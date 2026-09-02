namespace UniStart.Application.DTOs;


public record AdminMockSectionInputDto(
    int? ExamSectionId,
    string Name,
    int TimeLimitMinutes,
    int QuestionCount,
    int SortOrder,
    string? Instructions,
    List<int>? TopicOrder = null
);

public record AdminMockSectionDto(
    int Id,
    int? ExamSectionId,
    string Name,
    int TimeLimitMinutes,
    int QuestionCount,
    int SortOrder,
    string? Instructions,
    List<int>? TopicOrder = null
);

public record AdminMockExamListItemDto(
    int Id,
    string ExamTypeCode,
    string Title,
    string Description,
    int TotalTimeMinutes,
    bool IsActive,
    int SectionCount,
    int QuestionCount,
    int AttemptCount,
    string? TitleKz = null,
    string? TitleEn = null,
    string? DescriptionKz = null,
    string? DescriptionEn = null
);

public record AdminMockExamDetailDto(
    int Id,
    string ExamTypeCode,
    string Title,
    string Description,
    int TotalTimeMinutes,
    bool IsActive,
    IEnumerable<AdminMockSectionDto> Sections,
    string? TitleKz = null,
    string? TitleEn = null,
    string? DescriptionKz = null,
    string? DescriptionEn = null
);

public record SaveMockExamDto(
    string ExamTypeCode,
    string Title,
    string Description,
    int TotalTimeMinutes,
    bool IsActive,
    List<AdminMockSectionInputDto> Sections,
    string? TitleKz = null,
    string? TitleEn = null,
    string? DescriptionKz = null,
    string? DescriptionEn = null
);
