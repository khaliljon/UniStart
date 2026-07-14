namespace UniStart.Domain.Entities;

/// <summary>
/// A price tier for buying N runs of a specific mock template.
/// Prices live in our DB (admin-editable, dynamic). Polar is charged the amount
/// we compute from these tiers.
/// </summary>
public class MockPriceTier : IAuditable
{
    public int Id { get; set; }

    public int MockExamId { get; set; }

    /// <summary>Number of runs this tier grants (e.g. 1, 3, 5).</summary>
    public int Runs { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "KZT";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual MockExam MockExam { get; set; } = null!;
}
