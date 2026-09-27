namespace UniStart.Domain.Entities;

public class ExamSitting
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>Second exam day; null for single-day sittings.</summary>
    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
