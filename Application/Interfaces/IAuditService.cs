using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(int userId, string userEmail, string action, string entityType, string? entityId,
        object? oldValues = null, object? newValues = null, string? ipAddress = null);

    Task<AuditLogPagedResult> GetLogsAsync(
        string? action = null, string? entityType = null, int? userId = null,
        DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 50);
}
