namespace UniStart.Domain.Entities;

public class ExamSection
{
    public int Id { get; set; }
    public string ExamTypeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MinScore { get; set; }
    public int MaxScore { get; set; }

    // Navigation properties
    public virtual ExamType ExamType { get; set; } = null!;
    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
