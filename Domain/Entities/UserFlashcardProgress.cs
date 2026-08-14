namespace UniStart.Domain.Entities;

public class UserFlashcardProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FlashcardId { get; set; }
    public double EaseFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; } = 1;
    public int Repetitions { get; set; }
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; set; }
    public int LastQuality { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Flashcard Flashcard { get; set; } = null!;
}
