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
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int FlashcardSetId { get; set; }
    }

    public class UpdateFlashcardDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    public class ReviewFlashcardDto
    {
        public int FlashcardId { get; set; }
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
