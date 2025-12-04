using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs
{
    // DTOs для Flashcard
    public class FlashcardDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime? LastReviewedAt { get; set; }
        public bool IsDueForReview { get; set; }
    }

    public class CreateFlashcardDto
    {
        [Required(ErrorMessage = "Вопрос обязателен")]
        [StringLength(500, ErrorMessage = "Вопрос не должен превышать 500 символов")]
        public string Question { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Ответ обязателен")]
        [StringLength(500, ErrorMessage = "Ответ не должен превышать 500 символов")]
        public string Answer { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Объяснение не должно превышать 1000 символов")]
        public string Explanation { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "ID набора обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID набора должен быть больше 0")]
        public int FlashcardSetId { get; set; }
    }

    public class UpdateFlashcardDto
    {
        [Required(ErrorMessage = "Вопрос обязателен")]
        [StringLength(500, ErrorMessage = "Вопрос не должен превышать 500 символов")]
        public string Question { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Ответ обязателен")]
        [StringLength(500, ErrorMessage = "Ответ не должен превышать 500 символов")]
        public string Answer { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Объяснение не должно превышать 1000 символов")]
        public string Explanation { get; set; } = string.Empty;
    }

    public class ReviewFlashcardDto
    {
        [Required(ErrorMessage = "ID карточки обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID карточки должен быть больше 0")]
        public int FlashcardId { get; set; }
        
        [Required(ErrorMessage = "Оценка обязательна")]
        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Quality { get; set; } // 0-5 по алгоритму SM-2
    }

    public class ReviewResultDto
    {
        public int FlashcardId { get; set; }
        public DateTime NextReviewDate { get; set; }
        public int IntervalDays { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
