namespace UniStart.Domain.Entities;

/// <summary>
/// Interface for soft-deletable entities (OP-9).
/// Entities implementing this are filtered out by default via global query filters.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedBy { get; set; }
}
