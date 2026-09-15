namespace UniStart.Application.DTOs;

/// <summary>A single access grant captured at order time (mock runs to add).</summary>
public record OrderGrantSnapshot(int MockExamId, int Runs);

/// <summary>
/// Immutable per-line snapshot resolved on the server at checkout time. Access is later
/// granted strictly from this snapshot, never from the live catalog/prices.
/// </summary>
public record OrderLineSnapshot(
    string ItemType,
    string ItemCode,
    string Title,
    string? Subjects,
    decimal Price,
    string Language,
    List<OrderGrantSnapshot> Grants);

/// <summary>Full immutable server-resolved order snapshot stored on the PaymentOrder.</summary>
public record OrderSnapshot(
    decimal Total,
    string Currency,
    List<OrderLineSnapshot> Lines);
