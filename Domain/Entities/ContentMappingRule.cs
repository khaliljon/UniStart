namespace UniStart.Domain.Entities;

/// <summary>
/// Admin-editable rule that maps a Google Drive folder/file to a Skill, so the
/// Drive→content sync can be re-targeted WITHOUT a redeploy. When a rule matches a
/// file, its <see cref="SkillName"/> overrides the name-based heuristic. The optional
/// <see cref="Glossary"/> doubles as the unit description fed to the TSA classifier
/// prompt, improving how past-paper questions are distributed across units.
/// </summary>
public class ContentMappingRule
{
    public int Id { get; set; }

    /// <summary>
    /// Scope: only applies when the sync runs for this exam section (case-insensitive).
    /// Null/empty = applies to every section.
    /// </summary>
    public string? ExamSectionName { get; set; }

    /// <summary>What part of the Drive path the <see cref="Pattern"/> is matched against.</summary>
    public ContentMatchType MatchType { get; set; } = ContentMatchType.FolderSegment;

    /// <summary>
    /// Case-insensitive substring matched against the first folder segment
    /// (<see cref="ContentMatchType.FolderSegment"/>) or the file name without its
    /// extension (<see cref="ContentMatchType.FileName"/>).
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>The Skill name a matching file is ingested into.</summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Optional 1–2 line description of the unit/skill. When the skill is a TSA unit,
    /// this is injected into the classifier prompt as a glossary entry so questions
    /// are routed more accurately.
    /// </summary>
    public string? Glossary { get; set; }

    /// <summary>Lower runs first; the first active matching rule wins.</summary>
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum ContentMatchType
{
    /// <summary>Match against the first folder segment under the synced root.</summary>
    FolderSegment = 0,

    /// <summary>Match against the file name (without extension).</summary>
    FileName = 1
}
