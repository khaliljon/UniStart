namespace UniStart.Domain.Entities;

/// <summary>
/// Admin-managed mapping "field of study → CSCA subjects to take".
/// Replaces the previously hardcoded frontend list.
/// </summary>
public class SpecialtyTrack : IAuditable
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? NameKz { get; set; }
    public string? NameEn { get; set; }

    /// <summary>Comma-separated subject keys: math, physics, chemistry, chineseTech, chineseHum.</summary>
    public string Subjects { get; set; } = string.Empty;

    /// <summary>Subjects are only required when the programme is taught in Chinese.</summary>
    public bool ConditionalChinese { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
