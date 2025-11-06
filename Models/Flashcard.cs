namespace UniStart.Models
{
    public class Flashcard
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        
        // Spaced Repetition Algorithm (SM-2)
        public double EaseFactor { get; set; } = 2.5; // Начальный коэффициент легкости
        public int Interval { get; set; } = 0; // Интервал в днях до следующего повтора
        public int Repetitions { get; set; } = 0; // Количество успешных повторений
        public DateTime? NextReviewDate { get; set; } // Дата следующего повторения
        public DateTime? LastReviewedAt { get; set; } // Последняя проверка
        
        // Внешний ключ
        public int FlashcardSetId { get; set; }
        public FlashcardSet FlashcardSet { get; set; } = null!;
    }
}
