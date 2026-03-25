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

    // ── White Label Branding ──────────────────────────────
    public string? Subdomain { get; set; }          // e.g. "linhao" → linhao.unistart.kz
    public string? CustomDomain { get; set; }       // e.g. "prep.linhao.cn"
    public string? PrimaryColor { get; set; }       // e.g. "#c0392b"
    public string? PrimaryHoverColor { get; set; }  // e.g. "#e74c3c"
    public string? AccentColor { get; set; }        // e.g. "#d4a437"
    public string? NavbarTitle { get; set; }        // custom navbar text, defaults to Name

    // Navigation
    public virtual User? Owner { get; set; }
    public ICollection<TutorProfile> Tutors { get; set; } = new List<TutorProfile>();
}
