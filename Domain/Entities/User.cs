namespace UniStart.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    public virtual ICollection<UserSkillProfile> SkillProfiles { get; set; } = new List<UserSkillProfile>();
}

public enum UserRole
{
    Student,
    Tutor,
    Admin
}
