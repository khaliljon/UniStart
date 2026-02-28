namespace UniStart.Domain.Entities;

/// <summary>
/// Entities implementing this interface will have CreatedAt/UpdatedAt
/// automatically managed by DbContext.SaveChangesAsync override (OP-16).
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
