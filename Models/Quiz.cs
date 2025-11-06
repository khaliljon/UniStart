namespace UniStart.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeLimit { get; set; } // В минутах (0 = без ограничения)
        public string Subject { get; set; } = string.Empty; // Математика, Физика, и т.д.
        public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public List<Question> Questions { get; set; } = new();
        public List<UserQuizAttempt> Attempts { get; set; } = new();
    }
}
