namespace UniStart.Application.DTOs;


public record LessonStepDto(
    int Id,
    int LessonId,
    string Title,
    string Content,
    string StepType,
    int? QuizQuestionId,
    int SortOrder
);

public record LessonWithStepsDto(
    int Id,
    int TopicId,
    string TopicName,
    string Title,
    string? VideoUrl,
    IEnumerable<LessonStepDto> Steps,
    int CompletedSteps,
    int TotalSteps
);


public record FormulaCardDto(
    int Id,
    int TopicId,
    string TopicName,
    string? ExamTypeCode,
    string Title,
    string Formula,
    string? Description,
    bool IsBookmarked
);


public record FlashcardDeckDto(
    int Id,
    string Title,
    string? Description,
    string? ExamTypeCode,
    int? TopicId,
    bool IsSystem,
    int TotalCards,
    int DueCards,
    int MasteredCards
);

public record FlashcardDto(
    int Id,
    int DeckId,
    string Front,
    string Back,
    int SortOrder
);

public record FlashcardReviewDto(
    int FlashcardId,
    string Front,
    string Back,
    int? CurrentInterval,
    double? EaseFactor
);

public record ReviewFlashcardRequest(
    int FlashcardId,
    int Quality
);

public record CreateDeckRequest(
    string Title,
    string? Description,
    string? ExamTypeCode,
    int? TopicId
);

public record CreateFlashcardRequest(
    int DeckId,
    string Front,
    string Back
);


public record StartDrillRequest(
    string DrillType,
    string[]? ExamTypeCodes,
    int? TopicId,
    int? TimeLimitMinutes
);

public record DrillQuestionDto(
    int QuestionId,
    string Text,
    string? TopicName,
    string Difficulty,
    IEnumerable<DrillAnswerOptionDto> Options
);

public record DrillAnswerOptionDto(
    int Id,
    string Text
);

public record SubmitDrillAnswerRequest(
    int DrillResultId,
    int QuestionId,
    int AnswerOptionId,
    int TimeSpentSeconds
);

public record DrillAnswerResultDto(
    bool IsCorrect,
    int? CorrectOptionId,
    string? Explanation,
    int CurrentStreak,
    int TotalCorrect,
    int TotalAnswered,
    bool DrillEnded
);

public record DrillResultDto(
    int Id,
    string DrillType,
    int QuestionsAnswered,
    int CorrectAnswers,
    int TotalTimeSeconds,
    double AverageTimeSeconds,
    int BestStreak,
    double AccuracyPercent,
    DateTime CompletedAt
);

public record PersonalBestDto(
    string DrillType,
    int? BestScore,
    int? BestStreak,
    double? BestAverageTime,
    DateTime? AchievedAt
);


public record StrategyGuideDto(
    int Id,
    string ExamTypeCode,
    string Title,
    string Summary,
    string Content,
    string Category,
    int EstimatedReadMinutes,
    bool IsRead
);

public record StrategyGuideSummaryDto(
    int Id,
    string ExamTypeCode,
    string Title,
    string Summary,
    string Category,
    int EstimatedReadMinutes,
    bool IsRead
);


public record MistakeEntryDto(
    int UserAnswerId,
    int QuestionId,
    string QuestionText,
    string TopicName,
    string Difficulty,
    string? UserAnswerText,
    string? CorrectAnswerText,
    string? Explanation,
    DateTime AnsweredAt,
    string? ErrorType,
    string? NoteText
);

public record SetErrorTypeRequest(int UserAnswerId, string? ErrorType);
public record SetMistakeNoteRequest(int UserAnswerId, string NoteText);

public record ErrorPatternDto(
    string ErrorType,
    int Count,
    double Percentage
);

public record MistakeAnalysisDto(
    int TotalMistakes,
    IEnumerable<ErrorPatternDto> ErrorPatterns,
    IEnumerable<TopicMistakeDto> TopicBreakdown
);

public record TopicMistakeDto(
    int TopicId,
    string TopicName,
    int MistakeCount
);
