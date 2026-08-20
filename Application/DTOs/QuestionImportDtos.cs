namespace UniStart.Application.DTOs;


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
    List<ImportJobFileDto>? Files = null,
    string ContentType = "Questions",
    string? ResultSummary = null
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
    DateTime? ReviewedAt,
    string? ImageUrl
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
    double? IrtC,
    string? ImageUrl = null
);

/// <summary>Extracted raw question from a file (before saving to DB)</summary>
public record ExtractedQuestion(
    string QuestionText,
    List<DraftOptionDto> Options,
    string? Explanation,
    string? Hint,
    string? Difficulty,
    byte[]? ImageData = null,
    string? ImageContentType = null
);

// ─── Theory Import DTOs ──────────────────────────────────────

/// <summary>All theory content extracted from a single theory file.</summary>
public record ExtractedTheory(
    List<ExtractedLesson> Lessons,
    List<ExtractedFormula> Formulas,
    List<ExtractedFlashcard> Flashcards,
    List<ExtractedStrategy> Strategies
);

public record ExtractedLesson(string Title, string Content);

/// <summary>Formula with a KaTeX-formatted expression.</summary>
public record ExtractedFormula(string Title, string Formula, string? Description);

public record ExtractedFlashcard(string Front, string Back);

/// <summary>Strategy guide. Category is one of: test-taking, time-management, section-specific, mental.</summary>
public record ExtractedStrategy(string Title, string Summary, string Content, string Category);
