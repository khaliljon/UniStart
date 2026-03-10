namespace UniStart.Application.DTOs;

// ─── Admin Content Management DTOs ──────────────────────

// Lessons
public record AdminLessonListDto(
    int Id,
    int TopicId,
    string TopicName,
    string Title,
    string? VideoUrl,
    int SortOrder,
    int StepCount
);

public record AdminCreateLessonDto(
    int TopicId,
    string Title,
    string Content,
    string? VideoUrl,
    int SortOrder = 0
);

public record AdminUpdateLessonDto(
    string? Title,
    string? Content,
    string? VideoUrl,
    int? SortOrder
);

// Flashcard Decks
public record AdminDeckListDto(
    int Id,
    string Title,
    string? Description,
    string? ExamTypeCode,
    int? TopicId,
    string? TopicName,
    bool IsSystem,
    int CardCount,
    DateTime CreatedAt
);

public record AdminCreateDeckDto(
    string Title,
    string? Description,
    string? ExamTypeCode,
    int? TopicId
);

public record AdminUpdateDeckDto(
    string? Title,
    string? Description,
    string? ExamTypeCode,
    int? TopicId
);

// Flashcards
public record AdminFlashcardDto(
    int Id,
    int DeckId,
    string Front,
    string Back,
    int SortOrder
);

public record AdminCreateFlashcardDto(
    int DeckId,
    string Front,
    string Back,
    int SortOrder = 0
);

public record AdminUpdateFlashcardDto(
    string? Front,
    string? Back,
    int? SortOrder
);

// Formulas
public record AdminFormulaListDto(
    int Id,
    int TopicId,
    string TopicName,
    string Title,
    string Formula,
    string? Description,
    int SortOrder
);

public record AdminCreateFormulaDto(
    int TopicId,
    string Title,
    string Formula,
    string? Description,
    int SortOrder = 0
);

public record AdminUpdateFormulaDto(
    string? Title,
    string? Formula,
    string? Description,
    int? SortOrder
);

// Strategies
public record AdminStrategyListDto(
    int Id,
    string ExamTypeCode,
    string Title,
    string Summary,
    string Category,
    int EstimatedReadMinutes,
    int SortOrder
);

public record AdminCreateStrategyDto(
    string ExamTypeCode,
    string Title,
    string Summary,
    string Content,
    string Category,
    int EstimatedReadMinutes = 5,
    int SortOrder = 0
);

public record AdminUpdateStrategyDto(
    string? Title,
    string? Summary,
    string? Content,
    string? Category,
    int? EstimatedReadMinutes,
    int? SortOrder
);

// Drill Templates
public record AdminDrillTemplateListDto(
    int Id,
    string Title,
    string? Description,
    string DrillType,
    string? ExamTypeCode,
    int? TopicId,
    string? TopicName,
    int QuestionCount,
    int? TimeLimitMinutes,
    bool IsActive,
    int SortOrder
);

public record AdminCreateDrillTemplateDto(
    string Title,
    string? Description,
    string DrillType,
    string? ExamTypeCode,
    int? TopicId,
    int QuestionCount = 10,
    int? TimeLimitMinutes = null,
    bool IsActive = true,
    int SortOrder = 0
);

public record AdminUpdateDrillTemplateDto(
    string? Title,
    string? Description,
    string? DrillType,
    string? ExamTypeCode,
    int? TopicId,
    int? QuestionCount,
    int? TimeLimitMinutes,
    bool? IsActive,
    int? SortOrder
);
