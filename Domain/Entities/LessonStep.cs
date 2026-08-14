namespace UniStart.Domain.Entities;

public class LessonStep : IAuditable
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public LessonStepType StepType { get; set; } = LessonStepType.Theory;
    public int? QuizQuestionId { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual TopicLesson Lesson { get; set; } = null!;
    public virtual Question? QuizQuestion { get; set; }
    public virtual ICollection<UserLessonProgress> UserProgress { get; set; } = new List<UserLessonProgress>();
}

public enum LessonStepType
{
    Theory = 1,
    Example = 2,
    Quiz = 3,
    Summary = 4
}
