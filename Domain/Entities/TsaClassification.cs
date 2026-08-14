namespace UniStart.Domain.Entities;

public class TsaClassification
{
    public int Id { get; set; }

    public string QuestionHash { get; set; } = string.Empty;

    public string SkillName { get; set; } = string.Empty;

    public string? TopicName { get; set; }

    public string? ExamSectionName { get; set; }

    public string? QuestionPreview { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}
