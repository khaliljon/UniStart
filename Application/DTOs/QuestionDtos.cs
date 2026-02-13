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
    int AnswerOptionId
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
