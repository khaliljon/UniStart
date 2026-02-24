namespace UniStart.Domain.Entities;

public class TestSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ExamTypeCode { get; set; } = string.Empty;
    public string Mode { get; set; } = "practice"; // practice | exam
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public double? Score { get; set; } // Percentage score

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ExamType ExamType { get; set; } = null!;
    public virtual ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
}
