namespace UniStart.Domain.Entities;

public class MockPackage : IAuditable
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? NameKz { get; set; }
    public string? NameEn { get; set; }

    public int PickCount { get; set; }

    public int RunsEach { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "KZT";

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
