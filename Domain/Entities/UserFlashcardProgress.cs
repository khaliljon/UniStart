namespace UniStart.Domain.Entities;

/// <summary>
/// Tracks per-user flashcard progress using the SM-2 spaced repetition algorithm.
/// </summary>
public class UserFlashcardProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FlashcardId { get; set; }
    public double EaseFactor { get; set; } = 2.5;    // SM-2 ease factor (min 1.3)
    public int IntervalDays { get; set; } = 1;        // days until next review
    public int Repetitions { get; set; }               // consecutive correct reviews
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; set; }
    public int LastQuality { get; set; }               // 0-5 quality rating

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Flashcard Flashcard { get; set; } = null!;
}
