namespace UniStart.Domain.Entities;

public class TopicDependency
{
    public int TopicId { get; set; }
    public int PrerequisiteTopicId { get; set; }
    
    public double Weight { get; set; } = 1.0;

    public virtual Topic Topic { get; set; } = null!;
    public virtual Topic PrerequisiteTopic { get; set; } = null!;
}
