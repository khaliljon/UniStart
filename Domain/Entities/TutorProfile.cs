namespace UniStart.Domain.Entities;

public class TutorProfile : IAuditable
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // Profile info
    public string Headline { get; set; } = string.Empty;       // "Сертифицированный TOEFL-тьютор", ≤200
    public string Bio { get; set; } = string.Empty;             // ≤2000
    public string Experience { get; set; } = string.Empty;      // ≤1000
    public string? AvatarUrl { get; set; }

    // Specializations stored as comma-separated ExamTypeCodes: "SAT,TOEFL,NUET"
    public string Specializations { get; set; } = string.Empty;

    // Business
    public decimal? HourlyRate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsVerified { get; set; } = false;
    public ContactPreference ContactPreference { get; set; } = ContactPreference.Chat;

    // Cached aggregates (updated on review CRUD)
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public int TotalStudents { get; set; } = 0;

    // IAuditable
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // School affiliation (optional)
    public int? SchoolId { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual TutorSchool? School { get; set; }
    public virtual ICollection<TutorScheduleSlot> Schedule { get; set; } = new List<TutorScheduleSlot>();
    public virtual ICollection<TutorReview> Reviews { get; set; } = new List<TutorReview>();
}

public enum ContactPreference
{
    Chat,
    Email,
    Both
}
