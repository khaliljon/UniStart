namespace UniStart.Application.DTOs;

// Question DTOs
public record QuestionDto(
    int Id,
    string Text,
    string Difficulty,
    int TopicId,
    string TopicName,
    IEnumerable<AnswerOptionDto> Options
);

public record AnswerOptionDto(
    int Id,
    string Text
);

public record SubmitAnswerDto(
    int QuestionId,
    int AnswerOptionId,
    int? TimeSpentSeconds = null,
    int? TestSessionId = null
);

public record AnswerResultDto(
    bool IsCorrect,
    int CorrectOptionId,
    string CorrectOptionText,
    string? Explanation,
    int NewSkillLevel,
    int SkillChange
);

public record NextQuestionDto(
    QuestionDto? Question,
    bool TestCompleted,
    int QuestionsAnswered,
    int TotalQuestions
);

// Topic progress for topic selection page
public record TopicProgressDto(
    int TopicId,
    string TopicName,
    int TotalQuestions,
    int CorrectAnswers,
    int IncorrectAnswers,
    double MasteryPercentage
);
