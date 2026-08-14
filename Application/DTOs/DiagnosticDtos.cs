using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;


public record StartDiagnosticDto(
    [Required] string ExamTypeCode
);


public record DiagnosticSessionDto(
    int SessionId,
    string ExamTypeCode,
    string ExamTypeName,
    int TotalQuestions,
    int CurrentIndex,
    bool IsCompleted
);

public record DiagnosticQuestionDto(
    int Index,
    int TotalQuestions,
    int QuestionId,
    string Text,
    string Difficulty,
    string TopicName,
    string SectionName,
    IEnumerable<AnswerOptionDto> Options
);

public record DiagnosticAnswerDto(
    [Required] int SessionId,
    [Required] int QuestionId,
    [Required] int AnswerOptionId,
    int? TimeSpentSeconds = null
);

public record DiagnosticAnswerResultDto(
    bool IsCorrect,
    int CorrectOptionId,
    string CorrectOptionText,
    string? Explanation,
    int CurrentIndex,
    int TotalQuestions,
    bool IsCompleted
);

public record DiagnosticResultDto(
    int SessionId,
    string ExamTypeCode,
    string ExamTypeName,
    int TotalQuestions,
    int CorrectCount,
    double ScorePercent,
    int PredictedScore,
    int PredictedScoreMin,
    int PredictedScoreMax,
    int MaxPossibleScore,
    string Level,
    IEnumerable<DiagnosticSectionResultDto> SectionResults,
    IEnumerable<DiagnosticAnswerReviewDto> Answers
);

public record DiagnosticSectionResultDto(
    string SectionName,
    int TotalQuestions,
    int CorrectCount,
    double ScorePercent
);

public record DiagnosticAnswerReviewDto(
    int QuestionId,
    string QuestionText,
    string TopicName,
    string Difficulty,
    int SelectedOptionId,
    string SelectedOptionText,
    int CorrectOptionId,
    string CorrectOptionText,
    bool IsCorrect,
    string? Explanation
);
