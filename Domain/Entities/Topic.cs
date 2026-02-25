namespace UniStart.Domain.Entities;

public class Topic
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public int? SectionId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public virtual Skill Skill { get; set; } = null!;
    public virtual ExamSection? Section { get; set; }
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<TopicLesson> Lessons { get; set; } = new List<TopicLesson>();
    
    /// <summary>Topics that depend on this topic (this topic is their prerequisite)</summary>
    public virtual ICollection<TopicDependency> DependentTopics { get; set; } = new List<TopicDependency>();
    /// <summary>Topics that are prerequisites for this topic</summary>
    public virtual ICollection<TopicDependency> Prerequisites { get; set; } = new List<TopicDependency>();
}
