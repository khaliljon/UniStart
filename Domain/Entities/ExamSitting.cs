namespace UniStart.Domain.Entities;

/// <summary>
/// A CSCA exam sitting (date). Editable from the admin panel; shown on the
/// landing / "About CSCA" pages. The month label is derived from the date in the UI.
/// </summary>
public class ExamSitting
{
    public int Id { get; set; }

    /// <summary>Date of the exam sitting.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Whether this sitting is shown publicly.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Manual ordering (lower first). Ties broken by date.</summary>
    public int SortOrder { get; set; }
}
