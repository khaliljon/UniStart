namespace UniStart.Domain.Entities;

public class TutorScheduleSlot
{
    public int Id { get; set; }
    public int TutorProfileId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Navigation
    public virtual TutorProfile TutorProfile { get; set; } = null!;
}
