namespace UniStart.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "SingleChoice"; // SingleChoice, MultipleChoice, TrueFalse
        public int Points { get; set; } = 1;
        public int OrderIndex { get; set; }
        public string? ImageUrl { get; set; }
        public string? Explanation { get; set; } // Объяснение правильного ответа
        
        // Внешний ключ
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        
        // Навигационные свойства
        public List<Answer> Answers { get; set; } = new();
    }
}
