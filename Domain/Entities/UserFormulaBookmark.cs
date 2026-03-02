namespace UniStart.Domain.Entities;

public class UserFormulaBookmark
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FormulaCardId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual FormulaCard FormulaCard { get; set; } = null!;
}
