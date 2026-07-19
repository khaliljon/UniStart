namespace UniStart.Domain.Entities;

/// <summary>
/// A discounted bundle. The buyer picks <see cref="PickCount"/> subjects (mock
/// templates) and each selected template is granted <see cref="RunsEach"/> runs.
/// If <see cref="PickCount"/> is 0, all active templates are included.
/// </summary>
public class MockPackage : IAuditable
{
    public int Id { get; set; }

    /// <summary>Stable key, e.g. "start", "standard", "advanced", "full".</summary>
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Localized names (fallback to <see cref="Name"/> when empty).</summary>
    public string? NameKz { get; set; }
    public string? NameEn { get; set; }

    /// <summary>How many subjects the buyer selects. 0 means "all subjects".</summary>
    public int PickCount { get; set; }

    /// <summary>Runs granted to each selected subject.</summary>
    public int RunsEach { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "KZT";

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
