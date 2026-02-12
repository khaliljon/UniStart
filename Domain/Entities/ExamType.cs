namespace UniStart.Domain.Entities;

public class ExamType
{
    public string Code { get; set; } = string.Empty; // SAT, TOEFL, NUET
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<ExamSection> Sections { get; set; } = new List<ExamSection>();
}
