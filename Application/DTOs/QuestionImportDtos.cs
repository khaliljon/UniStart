namespace UniStart.Application.DTOs;

// ─── Question Import DTOs ────────────────────────────────────

public record QuestionImportJobDto(
    int Id,
    string FileName,
    string FileType,
    string ExamTypeCode,
    int? SectionId,
    int? TopicId,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int TotalExtracted,
    int TotalApproved,
    int TotalRejected,
    string? ErrorMessage,
    string? Instructions = null,
    List<ImportJobFileDto>? Files = null
);

public record ImportJobFileDto(
    int Id,
    string FileName,
    string FileType,
    string Role
);

public record ImportedQuestionDraftDto(
    int Id,
    int ImportJobId,
    string QuestionText,
    List<DraftOptionDto> Options,
    string? Explanation,
    string? Hint,
    int? TopicId,
    string? TopicName,
    string Difficulty,
    double IrtA,
    double IrtB,
    double IrtC,
    string Status,
    string Source,
    DateTime CreatedAt,
    DateTime? ReviewedAt
);

public record DraftOptionDto(
    string Text,
    bool IsCorrect
);

public record UpdateDraftDto(
    string? QuestionText,
    List<DraftOptionDto>? Options,
    string? Explanation,
    string? Hint,
    int? TopicId,
    string? Difficulty,
    double? IrtA,
    double? IrtB,
    double? IrtC
);

/// <summary>Extracted raw question from a file (before saving to DB)</summary>
public record ExtractedQuestion(
    string QuestionText,
    List<DraftOptionDto> Options,
    string? Explanation,
    string? Hint,
    string? Difficulty
);
