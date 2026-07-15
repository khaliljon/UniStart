namespace UniStart.Application.DTOs;

// ── Public catalog (витрина) ───────────────────────────────

/// <summary>A price tier for buying N runs of a template.</summary>
public record MockTierDto(int Id, int Runs, decimal Price, string Currency);

/// <summary>A mock template with its available run tiers and the user's balance.</summary>
public record MockTemplateDto(
    int MockExamId,
    string Title,
    string ExamTypeCode,
    int TotalQuestions,
    int TotalTimeMinutes,
    int RunsRemaining,
    IEnumerable<MockTierDto> Tiers);

/// <summary>A discounted package (user picks subjects).</summary>
public record MockPackageDto(
    int Id,
    string Key,
    string Name,
    int PickCount,
    int RunsEach,
    decimal Price,
    string Currency,
    int SortOrder);

/// <summary>Full catalog returned to the storefront.</summary>
public record MockCatalogDto(
    bool FreeRunAvailable,
    IEnumerable<MockTemplateDto> Templates,
    IEnumerable<MockPackageDto> Packages);

// ── Checkout (run-based) ───────────────────────────────────

/// <summary>One line in a checkout: either a run purchase or a package.</summary>
public class CheckoutLineDto
{
    /// <summary>"mock" | "package" | "book".</summary>
    public string Kind { get; set; } = string.Empty;

    // mock line
    public int? MockExamId { get; set; }
    public int Runs { get; set; }

    // package line
    public string? PackageKey { get; set; }
    public List<int>? SelectedMockIds { get; set; }

    // book line (per-material ownership)
    public int? BookMaterialId { get; set; }
}

/// <summary>A run-based checkout request (cart of lines).</summary>
public class RunCheckoutDto
{
    public List<CheckoutLineDto> Lines { get; set; } = new();
}

/// <summary>Priced, validated view of a checkout (used before payment).</summary>
public record CheckoutQuoteDto(decimal Total, string Currency);
