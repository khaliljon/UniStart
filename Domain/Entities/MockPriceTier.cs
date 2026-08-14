namespace UniStart.Domain.Entities;

public class MockPriceTier : IAuditable
{
    public int Id { get; set; }

    public int MockExamId { get; set; }

    public int Runs { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "KZT";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual MockExam MockExam { get; set; } = null!;
}
