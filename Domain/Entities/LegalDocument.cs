namespace UniStart.Domain.Entities;

/// <summary>
/// Editable legal document (privacy policy, user agreement, referral agreement).
/// Content is stored as plain text / lightweight markdown and rendered on public pages.
/// Editable from the admin panel.
/// </summary>
public class LegalDocument : IAuditable
{
    public int Id { get; set; }

    /// <summary>Stable identifier used in URLs / API: "privacy", "terms", "referral".</summary>
    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Human-readable "last updated" label shown on the page (e.g. "7 апреля 2026 г.").</summary>
    public string LastUpdatedLabel { get; set; } = string.Empty;

    /// <summary>Full document body (plain text / markdown).</summary>
    public string Content { get; set; } = string.Empty;

    // ── Localized variants (fallback to the base RU fields when empty) ──
    public string? TitleKz { get; set; }
    public string? TitleEn { get; set; }
    public string? ContentKz { get; set; }
    public string? ContentEn { get; set; }
    public string? LastUpdatedLabelKz { get; set; }
    public string? LastUpdatedLabelEn { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
