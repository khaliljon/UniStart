namespace UniStart.DTOs
{
    // DTOs для Quiz
    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public int TotalPoints { get; set; }
    }

    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuizDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public string? ImageUrl { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; } // Null при отправке пользователю
    }

    public class SubmitQuizDto
    {
        public int QuizId { get; set; }
        public int TimeSpentSeconds { get; set; }
        public Dictionary<int, List<int>> UserAnswers { get; set; } = new(); // QuestionId -> AnswerIds
    }

    public class QuizResultDto
    {
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public int TimeSpentSeconds { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new();
    }

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public int MaxPoints { get; set; }
        public List<int> CorrectAnswerIds { get; set; } = new();
        public List<int> UserAnswerIds { get; set; } = new();
        public string? Explanation { get; set; }
    }
}
