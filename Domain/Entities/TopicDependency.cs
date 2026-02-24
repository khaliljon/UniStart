namespace UniStart.Domain.Entities;

/// <summary>
/// Represents a prerequisite relationship between topics.
/// PrerequisiteTopicId must be mastered before TopicId.
/// </summary>
public class TopicDependency
{
    public int TopicId { get; set; }
    public int PrerequisiteTopicId { get; set; }
    
    /// <summary>Weight of the dependency (0.0 to 1.0), how strongly the prerequisite matters</summary>
    public double Weight { get; set; } = 1.0;

    // Navigation properties
    public virtual Topic Topic { get; set; } = null!;
    public virtual Topic PrerequisiteTopic { get; set; } = null!;
}
