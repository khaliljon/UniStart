namespace UniStart.Domain.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
    public virtual ICollection<UserSkillProfile> UserProfiles { get; set; } = new List<UserSkillProfile>();
}
