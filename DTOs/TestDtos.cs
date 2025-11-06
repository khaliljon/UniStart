namespace UniStart.DTOs;

/// <summary>
/// DTO для создания нового теста
/// </summary>
public class CreateTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    
    // Ограничения теста
    public int MaxAttempts { get; set; } = 3;
    public int PassingScore { get; set; } = 70;
    public bool IsProctored { get; set; } = false;
    public bool ShuffleQuestions { get; set; } = true;
    public bool ShuffleAnswers { get; set; } = true;
    
    // Настройки показа результатов
    public string ShowResultsAfter { get; set; } = "Immediate";
    public bool ShowCorrectAnswers { get; set; } = true;
    public bool ShowDetailedFeedback { get; set; } = true;
    
    // Временные ограничения
    public int TimeLimit { get; set; } = 60;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public List<CreateTestQuestionDto> Questions { get; set; } = new();
    public List<int> TagIds { get; set; } = new();
}

/// <summary>
/// DTO для вопроса теста при создании
/// </summary>
public class CreateTestQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int Points { get; set; } = 1;
    public int Order { get; set; }
    public List<CreateTestAnswerDto> Answers { get; set; } = new();
}

/// <summary>
/// DTO для варианта ответа при создании
/// </summary>
public class CreateTestAnswerDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// DTO для отображения теста
/// </summary>
public class TestDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    
    public int MaxAttempts { get; set; }
    public int PassingScore { get; set; }
    public bool IsProctored { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleAnswers { get; set; }
    
    public string ShowResultsAfter { get; set; } = string.Empty;
    public bool ShowCorrectAnswers { get; set; }
    public bool ShowDetailedFeedback { get; set; }
    
    public int TimeLimit { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public bool IsPublished { get; set; }
    public int TotalPoints { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// DTO для прохождения теста (без правильных ответов)
/// </summary>
public class TestTakingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int TimeLimit { get; set; }
    public int TotalPoints { get; set; }
    public int MaxAttempts { get; set; }
    public int RemainingAttempts { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleAnswers { get; set; }
    public List<TestQuestionTakingDto> Questions { get; set; } = new();
}

/// <summary>
/// DTO для вопроса при прохождении теста
/// </summary>
public class TestQuestionTakingDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Order { get; set; }
    public List<TestAnswerTakingDto> Answers { get; set; } = new();
}

/// <summary>
/// DTO для варианта ответа при прохождении
/// </summary>
public class TestAnswerTakingDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

/// <summary>
/// DTO для отправки ответов на тест
/// </summary>
public class SubmitTestDto
{
    public int TestId { get; set; }
    public List<TestAnswerSubmissionDto> Answers { get; set; } = new();
    public TimeSpan TimeSpent { get; set; }
}

/// <summary>
/// DTO для одного ответа
/// </summary>
public class TestAnswerSubmissionDto
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}

/// <summary>
/// DTO для результата теста
/// </summary>
public class TestResultDto
{
    public int AttemptId { get; set; }
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public DateTime CompletedAt { get; set; }
    public int AttemptNumber { get; set; }
    public int RemainingAttempts { get; set; }
    
    public bool ShowCorrectAnswers { get; set; }
    public bool ShowDetailedFeedback { get; set; }
    public List<TestQuestionResultDto>? QuestionResults { get; set; }
}

/// <summary>
/// DTO для результата по вопросу
/// </summary>
public class TestQuestionResultDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public int PointsEarned { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
    
    public int SelectedAnswerId { get; set; }
    public string SelectedAnswerText { get; set; } = string.Empty;
    public int? CorrectAnswerId { get; set; }
    public string? CorrectAnswerText { get; set; }
}

/// <summary>
/// DTO для статистики теста
/// </summary>
public class TestStatsDto
{
    public int TestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int UniqueStudents { get; set; }
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
    public TimeSpan AverageTimeSpent { get; set; }
    
    public List<TestQuestionStatsDto> QuestionStats { get; set; } = new();
    public List<TestAttemptSummaryDto> RecentAttempts { get; set; } = new();
}

/// <summary>
/// DTO для статистики по вопросу
/// </summary>
public class TestQuestionStatsDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int TotalAnswers { get; set; }
    public int CorrectAnswers { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// DTO для краткой информации о попытке
/// </summary>
public class TestAttemptSummaryDto
{
    public int AttemptId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public DateTime CompletedAt { get; set; }
}
