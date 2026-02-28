using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class AuditService : IAuditService
{
    private readonly UniStartDbContext _db;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditService(UniStartDbContext db, ILogger<AuditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(
        int userId, string userEmail, string action, string entityType, string? entityId,
        object? oldValues = null, object? newValues = null, string? ipAddress = null)
    {
        try
        {
            var entry = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues is not null ? JsonSerializer.Serialize(oldValues, JsonOpts) : null,
                NewValues = newValues is not null ? JsonSerializer.Serialize(newValues, JsonOpts) : null,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Audit logging should never crash the main operation
            _logger.LogError(ex, "Failed to write audit log: {Action} {EntityType} {EntityId}",
                action, entityType, entityId);
        }
    }

    public async Task<AuditLogPagedResult> GetLogsAsync(
        string? action = null, string? entityType = null, int? userId = null,
        DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(1, page);

        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.UserId,
                a.UserEmail,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.IpAddress,
                a.Timestamp
            ))
            .ToListAsync();

        return new AuditLogPagedResult(items, totalCount, page, pageSize, totalPages);
    }
}
