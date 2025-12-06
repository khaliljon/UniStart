using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs;

/// <summary>
/// DTO для создания нового экзамена
/// </summary>
public class CreateExamDto
{
    [Required(ErrorMessage = "Название экзамена обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Предмет обязателен")]
    [StringLength(100, ErrorMessage = "Название предмета не должно превышать 100 символов")]
    public string Subject { get; set; } = string.Empty;
    
    public string Difficulty { get; set; } = "Medium";
    
    // Ограничения экзамена
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
    
    public bool IsPublished { get; set; } = false;
    
    public List<CreateExamQuestionDto> Questions { get; set; } = new();
    public List<int> TagIds { get; set; } = new();
}

/// <summary>
/// DTO для вопроса экзамена при создании
/// </summary>
public class CreateExamQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int Points { get; set; } = 1;
    public int Order { get; set; }
    public List<CreateExamAnswerDto> Answers { get; set; } = new();
}

/// <summary>
/// DTO для варианта ответа при создании
/// </summary>
public class CreateExamAnswerDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// DTO для отображения экзамена
/// </summary>
public class ExamDto
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
/// DTO для прохождения экзамена (без правильных ответов)
/// </summary>
public class ExamTakingDto
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
    public List<ExamQuestionTakingDto> Questions { get; set; } = new();
}

/// <summary>
/// DTO для вопроса при прохождении экзамена
/// </summary>
public class ExamQuestionTakingDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Order { get; set; }
    public List<ExamAnswerTakingDto> Answers { get; set; } = new();
}

/// <summary>
/// DTO для варианта ответа при прохождении
/// </summary>
public class ExamAnswerTakingDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

/// <summary>
/// DTO для отправки ответов на экзамен
/// </summary>
public class SubmitExamDto
{
    public int ExamId { get; set; }
    public List<ExamAnswerSubmissionDto> Answers { get; set; } = new();
    public TimeSpan TimeSpent { get; set; }
}

/// <summary>
/// DTO для одного ответа
/// </summary>
public class ExamAnswerSubmissionDto
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}

/// <summary>
/// DTO для результата экзамена
/// </summary>
public class ExamResultDto
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
    public List<ExamQuestionResultDto>? QuestionResults { get; set; }
}

/// <summary>
/// DTO для результата по вопросу
/// </summary>
public class ExamQuestionResultDto
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
/// DTO для статистики экзамена
/// </summary>
public class ExamStatsDto
{
    public int ExamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int UniqueStudents { get; set; }
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
    public TimeSpan AverageTimeSpent { get; set; }
    
    public List<ExamQuestionStatsDto> QuestionStats { get; set; } = new();
    public List<ExamAttemptSummaryDto> RecentAttempts { get; set; } = new();
}

/// <summary>
/// DTO для статистики по вопросу
/// </summary>
public class ExamQuestionStatsDto
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
public class ExamAttemptSummaryDto
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
