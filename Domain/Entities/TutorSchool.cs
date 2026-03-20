namespace UniStart.Domain.Entities;

public class TutorSchool
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TelegramUrl { get; set; }
    public string Specializations { get; set; } = string.Empty; // comma-separated exam codes
    public bool IsPartner { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int? OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual User? Owner { get; set; }
    public ICollection<TutorProfile> Tutors { get; set; } = new List<TutorProfile>();
}
