namespace UniStart.Application.DTOs;

// ─── Admin Question Management ──────────────────────────

public record QuestionListDto(
    int Id,
    string TopicName,
    string SectionName,
    string ExamTypeCode,
    string Text,
    string Difficulty,
    double DifficultyParam,
    double DiscriminationParam,
    int AnswerCount,
    DateTime CreatedAt
);

public record QuestionDetailDto(
    int Id,
    int TopicId,
    string TopicName,
    string SectionName,
    string ExamTypeCode,
    string Text,
    string Difficulty,
    string? Explanation,
    double DifficultyParam,
    double DiscriminationParam,
    double GuessParam,
    DateTime CreatedAt,
    List<AdminAnswerOptionDto> AnswerOptions
);

public record AdminAnswerOptionDto(
    int Id,
    string Text,
    bool IsCorrect
);

public record CreateQuestionDto(
    int TopicId,
    string Text,
    string Difficulty,  // "Easy", "Medium", "Hard"
    string? Explanation,
    double? DifficultyParam,
    double? DiscriminationParam,
    double? GuessParam,
    List<CreateAnswerOptionDto> AnswerOptions
);

public record CreateAnswerOptionDto(
    string Text,
    bool IsCorrect
);

public record UpdateQuestionDto(
    string? Text,
    string? Difficulty,
    string? Explanation,
    double? DifficultyParam,
    double? DiscriminationParam,
    double? GuessParam,
    List<CreateAnswerOptionDto>? AnswerOptions
);

public record BulkImportDto(
    List<CreateQuestionDto> Questions
);

public record BulkImportResultDto(
    int Total,
    int Imported,
    int Failed,
    List<string> Errors
);

public record QuestionStatsDto(
    int TotalQuestions,
    Dictionary<string, int> ByExam,
    Dictionary<string, int> ByDifficulty,
    Dictionary<string, int> ByTopic,
    int TopicsWithQuestions,
    int TopicsWithoutQuestions
);
