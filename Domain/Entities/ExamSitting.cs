namespace UniStart.Domain.Entities;

public class ExamSitting
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
