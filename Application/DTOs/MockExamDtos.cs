namespace UniStart.Application.DTOs;

// ═══════════════════════════════════════════════════════
//  MOCK EXAM DTOs
// ═══════════════════════════════════════════════════════

/// <summary>Mock exam listing item</summary>
public record MockExamListDto(
    int Id,
    string ExamTypeCode,
    string ExamTypeName,
    string Title,
    string Description,
    int TotalTimeMinutes,
    int SectionCount,
    int TotalQuestions,
    int? BestScore,      // User's best score (null if never attempted)
    int AttemptCount,     // How many times user attempted
    int RunsRemaining,    // Paid runs left for this template (run-based model)
    bool FreeAvailable    // User still has their one free run (same for every row)
);

/// <summary>Full mock exam detail with sections</summary>
public record MockExamDetailDto(
    int Id,
    string ExamTypeCode,
    string ExamTypeName,
    string Title,
    string Description,
    int TotalTimeMinutes,
    IEnumerable<MockExamSectionDto> Sections
);

/// <summary>Section within a mock exam</summary>
public record MockExamSectionDto(
    int Id,
    string Name,
    int TimeLimitMinutes,
    int QuestionCount,
    int SortOrder,
    string? Instructions
);

/// <summary>Starting a mock exam attempt — returns attempt info + first section</summary>
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

/// <summary>Section state during an active attempt</summary>
public record MockExamSectionStateDto(
    int SectionIndex,
    string SectionName,
    int TimeLimitMinutes,
    string? Instructions,
    IEnumerable<MockExamQuestionDto> Questions,
    int TotalQuestions,
    int AnsweredCount
);

/// <summary>Question within a mock exam section — includes passage for TOEFL-style reading</summary>
public record MockExamQuestionDto(
    int QuestionId,
    string Text,
    string Difficulty,
    string TopicName,
    IEnumerable<MockExamOptionDto> Options,
    int? SelectedOptionId,      // Previously selected answer (for navigation)
    int? ReadingPassageId,
    string? PassageTitle,
    string? PassageContent,
    string? ImageUrl
);

/// <summary>Answer option</summary>
public record MockExamOptionDto(
    int Id,
    string Text
);

/// <summary>Submit or update an answer within a mock exam</summary>
public record MockExamSubmitAnswerDto(
    int QuestionId,
    int SelectedOptionId,
    int? TimeSpentSeconds = null
);

/// <summary>Section result after completion</summary>
public record MockExamSectionResultDto(
    int SectionIndex,
    string SectionName,
    int TotalQuestions,
    int CorrectCount,
    int UnansweredCount,
    double Accuracy,
    int TimeLimitMinutes
);

/// <summary>Complete mock exam results</summary>
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

/// <summary>Individual answer review in results</summary>
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
    string? Explanation
);

/// <summary>User's mock exam history item</summary>
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
