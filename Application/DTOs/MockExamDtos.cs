namespace UniStart.Application.DTOs;

public record MockExamListDto(
    int Id,
    string ExamTypeCode,
    string ExamTypeName,
    string Title,
    string Description,
    int TotalTimeMinutes,
    int SectionCount,
    int TotalQuestions,
    int? BestScore,
    int AttemptCount,
    int RunsRemaining,
    bool FreeAvailable,
    string? TitleKz = null,
    string? TitleEn = null,
    string? DescriptionKz = null,
    string? DescriptionEn = null
);

public record MockExamDetailDto(
    int Id,
    string ExamTypeCode,
    string ExamTypeName,
    string Title,
    string Description,
    int TotalTimeMinutes,
    IEnumerable<MockExamSectionDto> Sections,
    string? TitleKz = null,
    string? TitleEn = null,
    string? DescriptionKz = null,
    string? DescriptionEn = null
);

public record MockExamSectionDto(
    int Id,
    string Name,
    int TimeLimitMinutes,
    int QuestionCount,
    int SortOrder,
    string? Instructions
);

public record MockExamAttemptDto(
    int AttemptId,
    int MockExamId,
    string ExamTitle,
    string Status,
    int CurrentSectionIndex,
    int TotalSections,
    DateTime StartedAt,
    int TotalTimeMinutes,
    IEnumerable<string>? SectionNames = null
);

public record MockExamSectionStateDto(
    int SectionIndex,
    string SectionName,
    int TimeLimitMinutes,
    string? Instructions,
    IEnumerable<MockExamQuestionDto> Questions,
    int TotalQuestions,
    int AnsweredCount
);

public record MockExamQuestionDto(
    int QuestionId,
    string Text,
    string Difficulty,
    string TopicName,
    IEnumerable<MockExamOptionDto> Options,
    int? SelectedOptionId,
    int? ReadingPassageId,
    string? PassageTitle,
    string? PassageContent,
    string? ImageUrl,
    bool IsMultipleChoice = false,
    IEnumerable<int>? SelectedOptionIds = null
);

public record MockExamOptionDto(
    int Id,
    string Text
);

public record MockExamSubmitAnswerDto(
    int QuestionId,
    int SelectedOptionId,
    int? TimeSpentSeconds = null,
    List<int>? SelectedOptionIds = null
);

public record MockExamSectionResultDto(
    int SectionIndex,
    string SectionName,
    int TotalQuestions,
    int CorrectCount,
    int UnansweredCount,
    double Accuracy,
    int TimeLimitMinutes
);

public record MockExamResultDto(
    int AttemptId,
    int MockExamId,
    string ExamTitle,
    string ExamTypeCode,
    double TotalScore,
    int TotalCorrect,
    int TotalQuestions,
    double OverallAccuracy,
    int TotalTimeMinutes,
    DateTime StartedAt,
    DateTime? CompletedAt,
    IEnumerable<MockExamSectionResultDto> SectionResults,
    IEnumerable<MockExamAnswerReviewDto> AnswerReview
);

public record MockExamAnswerReviewDto(
    int QuestionId,
    string QuestionText,
    string TopicName,
    string Difficulty,
    string SectionName,
    int? SelectedOptionId,
    string? SelectedOptionText,
    int CorrectOptionId,
    string CorrectOptionText,
    bool IsCorrect,
    bool IsUnanswered,
    string? Explanation,
    bool IsMultipleChoice = false,
    IEnumerable<int>? SelectedOptionIds = null,
    IEnumerable<int>? CorrectOptionIds = null,
    string? ImageUrl = null
);

public record MockExamHistoryDto(
    int AttemptId,
    int MockExamId,
    string ExamTitle,
    string ExamTypeCode,
    string Status,
    double? TotalScore,
    DateTime StartedAt,
    DateTime? CompletedAt
);
