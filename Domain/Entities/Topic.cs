namespace UniStart.Domain.Entities;

public class Topic
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public int? SectionId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public virtual Skill Skill { get; set; } = null!;
    public virtual ExamSection? Section { get; set; }
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
