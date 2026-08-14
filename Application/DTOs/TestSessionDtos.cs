namespace UniStart.Application.DTOs;

public record StartTestSessionDto(
    string[] ExamTypeCodes,
    int? SectionId = null,
    int[]? SectionIds = null,
    int? TopicId = null
);

public record TestSessionDto(
    string SessionId,
    string[] ExamTypeCodes,
    int? SectionId,
    int CurrentQuestionIndex,
    int TotalQuestions,
    bool IsCompleted
);
