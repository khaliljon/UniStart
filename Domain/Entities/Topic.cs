namespace UniStart.Domain.Entities;

public class Topic : IAuditable
{
    public int Id { get; set; }
    public int? SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ExamSection? Section { get; set; }
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<TopicLesson> Lessons { get; set; } = new List<TopicLesson>();
    
    public virtual ICollection<TopicDependency> DependentTopics { get; set; } = new List<TopicDependency>();
    public virtual ICollection<TopicDependency> Prerequisites { get; set; } = new List<TopicDependency>();
}
