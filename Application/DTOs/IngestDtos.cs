namespace UniStart.Application.DTOs;

public record IngestContentDto(
    string ExamTypeCode,
    string ExamSectionName,
    string SkillName,
    List<IngestTopicDto>? Topics
);

public record IngestTopicDto(
    string? Name,
    int SortOrder,
    string? LessonContent,
    List<IngestFormulaDto>? Formulas,
    List<IngestQuestionDto>? Questions
);

public record IngestFormulaDto(
    string Title,
    string Formula,
    string? Description,
    int SortOrder
);

public record IngestQuestionDto(
    string Text,
    List<IngestOptionDto>? Options,
    string? Explanation,
    string? Hint,
    string Difficulty,
    int SortOrder
);

public record IngestOptionDto(
    string Text,
    bool IsCorrect
);
