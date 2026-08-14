namespace UniStart.Domain.Entities;

public class ExamType
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<ExamSection> Sections { get; set; } = new List<ExamSection>();
}
