namespace UniStart.Domain.Entities;

public enum StudyEntryType
{
    New,
    Review,
    Practice,
    Weakness
}

public class StudyPlanEntry
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int TopicId { get; set; }
    public DateTime Date { get; set; }
    public int RecommendedMinutes { get; set; }
    public StudyEntryType Type { get; set; } = StudyEntryType.New;
    public int RecommendedQuestions { get; set; } = 5;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int QuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }

    public virtual StudyPlan Plan { get; set; } = null!;
    public virtual Topic Topic { get; set; } = null!;
}
