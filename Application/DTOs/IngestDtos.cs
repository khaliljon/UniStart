namespace UniStart.Application.DTOs;

// ─── Study-Pack Ingest DTOs ──────────────────────────────────
// Normalized shape produced by the LLM study-pack parser
// (see StudyPackParserService). Hierarchy: Content → Topics → Questions/Formulas.

/// <summary>One ingestible skill payload: a flat list of concept-topics for a
/// given exam type / section / skill.</summary>
public record IngestContentDto(
    string ExamTypeCode,
    string ExamSectionName,
    string SkillName,
    List<IngestTopicDto>? Topics
);

/// <summary>A single concept. Carries its theory (lesson), formulas and the
/// questions classified onto it.</summary>
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
