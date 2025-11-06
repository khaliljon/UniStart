namespace UniStart.Models
{
    public class UserQuizAttempt
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Пока строка, позже будет связь с Identity
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public int TimeSpentSeconds { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        // Внешний ключ
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        
        // JSON с ответами пользователя
        public string UserAnswersJson { get; set; } = "{}"; // {"questionId": [answerId1, answerId2]}
    }
}
