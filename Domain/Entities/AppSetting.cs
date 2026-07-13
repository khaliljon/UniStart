namespace UniStart.Domain.Entities;

/// <summary>
/// Generic key/value application setting (admin-editable at runtime).
/// Used for pricing (mock price, material price, currency) and similar
/// global values so the frontend never hard-codes them.
/// </summary>
public class AppSetting : IAuditable
{
    public int Id { get; set; }

    /// <summary>Stable key, e.g. "MockPrice", "MaterialPrice", "Currency".</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Raw string value; callers parse to the type they need.</summary>
    public string Value { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
