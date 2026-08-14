namespace UniStart.Application.DTOs;

public record MockTierDto(int Id, int Runs, decimal Price, string Currency);

public record MockTemplateDto(
    int MockExamId,
    string Title,
    string ExamTypeCode,
    int TotalQuestions,
    int TotalTimeMinutes,
    int RunsRemaining,
    IEnumerable<MockTierDto> Tiers,
    string? TitleKz = null,
    string? TitleEn = null,
    string? Description = null,
    string? DescriptionKz = null,
    string? DescriptionEn = null);

public record MockPackageDto(
    int Id,
    string Key,
    string Name,
    int PickCount,
    int RunsEach,
    decimal Price,
    string Currency,
    int SortOrder,
    string? NameKz = null,
    string? NameEn = null);

public record MockCatalogDto(
    bool FreeRunAvailable,
    IEnumerable<MockTemplateDto> Templates,
    IEnumerable<MockPackageDto> Packages);

public class CheckoutLineDto
{
    public string Kind { get; set; } = string.Empty;

    public int? MockExamId { get; set; }
    public int Runs { get; set; }

    public string? PackageKey { get; set; }
    public List<int>? SelectedMockIds { get; set; }

    public int? BookMaterialId { get; set; }
}

public class RunCheckoutDto
{
    public List<CheckoutLineDto> Lines { get; set; } = new();
}

public record CheckoutQuoteDto(decimal Total, string Currency);
