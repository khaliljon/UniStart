using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Flashcards
{
    public class Flashcard
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Тип карточки")]
        public FlashcardType Type { get; set; } = FlashcardType.SingleChoice;
        
        [Display(Name = "Вопрос")]
        [Required(ErrorMessage = "Вопрос обязателен")]
        [StringLength(500, ErrorMessage = "Вопрос не должен превышать 500 символов")]
        public string Question { get; set; } = string.Empty;
        
        [Display(Name = "Ответ")]
        [Required(ErrorMessage = "Ответ обязателен")]
        [StringLength(500, ErrorMessage = "Ответ не должен превышать 500 символов")]
        public string Answer { get; set; } = string.Empty;
        
        [Display(Name = "Варианты ответов (JSON)")]
        [StringLength(2000)]
        public string? OptionsJson { get; set; } // Для Single Choice: ["opt1", "opt2", "opt3", "correct"]
        
        [Display(Name = "Пары для сопоставления (JSON)")]
        [StringLength(2000)]
        public string? MatchingPairsJson { get; set; } // Для Matching: [{"left":"term1","right":"def1"}]
        
        [Display(Name = "Последовательность (JSON)")]
        [StringLength(2000)]
        public string? SequenceJson { get; set; } // Для Sequencing: ["step1", "step2", "step3"]
        
        [Display(Name = "Объяснение")]
        [StringLength(1000, ErrorMessage = "Объяснение не должно превышать 1000 символов")]
        public string Explanation { get; set; } = string.Empty;
        
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; }
        
        // Spaced Repetition Algorithm (SM-2) параметры
        // ВНИМАНИЕ: Эти поля устарели и используются только для обратной совместимости.
        // Новый код должен использовать UserFlashcardProgress для хранения индивидуального прогресса пользователя.
        // Эти поля будут удалены в будущей версии после полной миграции данных.
        
        [Display(Name = "Коэффициент легкости - УСТАРЕВШЕЕ")]
        [Range(1.3, 5.0, ErrorMessage = "Коэффициент легкости должен быть от 1.3 до 5.0")]
        [Obsolete("Используйте UserFlashcardProgress.EaseFactor")]
        public double EaseFactor { get; set; } = 2.5; // Используется только для обратной совместимости
        
        [Display(Name = "Интервал повторения (дни) - УСТАРЕВШЕЕ")]
        [Range(0, int.MaxValue, ErrorMessage = "Интервал должен быть положительным")]
        [Obsolete("Используйте UserFlashcardProgress.Interval")]
        public int Interval { get; set; } = 0; // Используется только для обратной совместимости
        
        [Display(Name = "Количество повторений - УСТАРЕВШЕЕ")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество повторений должно быть положительным")]
        [Obsolete("Используйте UserFlashcardProgress.Repetitions")]
        public int Repetitions { get; set; } = 0; // Используется только для обратной совместимости
        
        [Display(Name = "Дата следующего повторения - УСТАРЕВШЕЕ")]
        [Obsolete("Используйте UserFlashcardProgress.NextReviewDate")]
        public DateTime? NextReviewDate { get; set; } // Используется только для обратной совместимости
        
        [Display(Name = "Последняя проверка - УСТАРЕВШЕЕ")]
        [Obsolete("Используйте UserFlashcardProgress.LastReviewedAt")]
        public DateTime? LastReviewedAt { get; set; } // Используется только для обратной совместимости
        
        // Внешний ключ
        [Display(Name = "ID набора")]
        public int FlashcardSetId { get; set; }
        
        [Display(Name = "Набор карточек")]
        public FlashcardSet FlashcardSet { get; set; } = null!;
    }
}
