namespace UniStart.Domain.Entities;

/// <summary>
/// Durable record of how a single TSA past-paper question was classified into a
/// Critical-Thinking unit, keyed by a hash of the normalized question text. Lets the
/// Drive sync persist and audit per-question unit assignments across re-runs, and is
/// the basis for skipping re-classification of questions that have not changed.
/// </summary>
public class TsaClassification
{
    public int Id { get; set; }

    /// <summary>SHA-256 (hex) of the normalized question text. Unique.</summary>
    public string QuestionHash { get; set; } = string.Empty;

    /// <summary>The unit (Skill) the question was assigned to, e.g. "Unit 3".</summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>The concept/topic within the unit, e.g. "Identifying Assumptions".</summary>
    public string? TopicName { get; set; }

    /// <summary>Exam section the classification belongs to (e.g. "Critical thinking").</summary>
    public string? ExamSectionName { get; set; }

    /// <summary>Short preview of the question text for the audit screen.</summary>
    public string? QuestionPreview { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}
