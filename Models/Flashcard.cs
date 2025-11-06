using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
{
    public class Flashcard
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Вопрос")]
        [Required(ErrorMessage = "Вопрос обязателен")]
        [StringLength(500, ErrorMessage = "Вопрос не должен превышать 500 символов")]
        public string Question { get; set; } = string.Empty;
        
        [Display(Name = "Ответ")]
        [Required(ErrorMessage = "Ответ обязателен")]
        [StringLength(500, ErrorMessage = "Ответ не должен превышать 500 символов")]
        public string Answer { get; set; } = string.Empty;
        
        [Display(Name = "Объяснение")]
        [StringLength(1000, ErrorMessage = "Объяснение не должно превышать 1000 символов")]
        public string Explanation { get; set; } = string.Empty;
        
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; }
        
        // Spaced Repetition Algorithm (SM-2)
        [Display(Name = "Коэффициент легкости")]
        [Range(1.3, 5.0, ErrorMessage = "Коэффициент легкости должен быть от 1.3 до 5.0")]
        public double EaseFactor { get; set; } = 2.5; // Начальный коэффициент легкости
        
        [Display(Name = "Интервал повторения (дни)")]
        [Range(0, int.MaxValue, ErrorMessage = "Интервал должен быть положительным")]
        public int Interval { get; set; } = 0; // Интервал в днях до следующего повтора
        
        [Display(Name = "Количество повторений")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество повторений должно быть положительным")]
        public int Repetitions { get; set; } = 0; // Количество успешных повторений
        
        [Display(Name = "Дата следующего повторения")]
        public DateTime? NextReviewDate { get; set; } // Дата следующего повторения
        
        [Display(Name = "Последняя проверка")]
        public DateTime? LastReviewedAt { get; set; } // Последняя проверка
        
        // Внешний ключ
        [Display(Name = "ID набора")]
        public int FlashcardSetId { get; set; }
        
        [Display(Name = "Набор карточек")]
        public FlashcardSet FlashcardSet { get; set; } = null!;
    }
}
