using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class PaymentOrderService : IPaymentOrderService
{
    private readonly UniStartDbContext _db;
    private readonly IEntitlementService _entitlements;
    private readonly IEmailService _email;
    private readonly ILogger<PaymentOrderService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public PaymentOrderService(UniStartDbContext db, IEntitlementService entitlements, IEmailService email, ILogger<PaymentOrderService> logger)
    {
        _db = db;
        _entitlements = entitlements;
        _email = email;
        _logger = logger;
    }

    public async Task<PaymentOrder> CreateAsync(int userId, string provider, List<CheckoutLineDto> lines, string? checkoutRef = null, string? requiredCurrency = null)
    {
        // Immutable server-resolved snapshot (trusted price + exact grants). Never the
        // value the frontend computed, and never re-resolved from the live catalog later.
        var snapshot = await _entitlements.ResolveSnapshotAsync(lines);

        // Validate currency BEFORE persisting so an unsupported currency leaves no Pending row.
        if (requiredCurrency != null && !string.Equals(snapshot.Currency, requiredCurrency, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Оплата возможна только в валюте {requiredCurrency}.");

        var order = new PaymentOrder
        {
            OrderCode = await GenerateUniqueOrderCodeAsync(provider),
            UserId = userId,
            Provider = provider,
            Status = PaymentOrderStatuses.Pending,
            Amount = snapshot.Total,
            Currency = snapshot.Currency,
            LinesJson = JsonSerializer.Serialize(snapshot, JsonOpts),
            CheckoutRef = checkoutRef,
        };

        _db.PaymentOrders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public OrderSnapshot DeserializeSnapshot(PaymentOrder order)
    {
        if (string.IsNullOrWhiteSpace(order.LinesJson))
            return new OrderSnapshot(order.Amount, order.Currency, new List<OrderLineSnapshot>());
        try
        {
            return JsonSerializer.Deserialize<OrderSnapshot>(order.LinesJson, JsonOpts)
                   ?? new OrderSnapshot(order.Amount, order.Currency, new List<OrderLineSnapshot>());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize snapshot for order {OrderCode}", order.OrderCode);
            return new OrderSnapshot(order.Amount, order.Currency, new List<OrderLineSnapshot>());
        }
    }

    public async Task<ConfirmResult> ConfirmAndGrantAsync(PaymentOrder order, string externalPaymentId, PurchaseAmountsDto? amounts = null)
    {
        // Reject reuse of the same external payment id across a different order.
        var duplicate = await _db.PaymentOrders.AnyAsync(o =>
            o.Id != order.Id && o.Provider == order.Provider && o.ExternalPaymentId == externalPaymentId);
        if (duplicate)
        {
            _logger.LogWarning("Payment id {PaymentId} already tied to another {Provider} order; order {OrderCode} rejected",
                externalPaymentId, order.Provider, order.OrderCode);
            return ConfirmResult.DuplicatePaymentId;
        }

        var snapshot = DeserializeSnapshot(order);
        if (snapshot.Lines.Count == 0)
            throw new InvalidOperationException($"Order {order.OrderCode} has no stored snapshot lines.");

        var now = DateTime.UtcNow;
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Atomic claim: only the first caller flips Pending → Paid. Concurrent/duplicate
            // callers get affected == 0 and skip granting, so access is granted exactly once.
            var affected = await _db.PaymentOrders
                .Where(o => o.Id == order.Id && o.Status == PaymentOrderStatuses.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.Status, PaymentOrderStatuses.Paid)
                    .SetProperty(o => o.ExternalPaymentId, externalPaymentId)
                    .SetProperty(o => o.PaidAt, now)
                    .SetProperty(o => o.UpdatedAt, now));

            if (affected == 0)
            {
                await tx.RollbackAsync();
                return ConfirmResult.AlreadyProcessed;
            }

            await _entitlements.GrantFromSnapshotAsync(order.UserId, snapshot, amounts, externalPaymentId, order.CheckoutRef, order);
            await tx.CommitAsync();

            order.Status = PaymentOrderStatuses.Paid;
            order.ExternalPaymentId = externalPaymentId;
            order.PaidAt = now;
            return ConfirmResult.Granted;
        }
        catch (DbUpdateException ex) when (IsProviderPaymentIdUniqueViolation(ex))
        {
            await tx.RollbackAsync();
            // Lost race on the partial unique (Provider, ExternalPaymentId) index.
            _logger.LogWarning(ex, "Confirm race for order {OrderCode}; treating payment id as duplicate", order.OrderCode);
            return ConfirmResult.DuplicatePaymentId;
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Confirm failed for order {OrderCode}", order.OrderCode);
            throw;
        }
    }

    // True only for a real Postgres unique violation (23505) on the provider/payment-id index.
    private static bool IsProviderPaymentIdUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg
        && pg.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(pg.ConstraintName, "IX_PaymentOrders_Provider_ExternalPaymentId", StringComparison.Ordinal);

    public async Task SendPurchaseReceiptForOrderAsync(PaymentOrder order)
    {
        try
        {
            var user = order.User ?? await _db.Users.FirstOrDefaultAsync(u => u.Id == order.UserId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var name = string.IsNullOrWhiteSpace(user.Name) ? user.FirstName : user.Name;
                await _email.SendPurchaseReceiptAsync(user.Email, name, order.Amount, order.Currency);
            }
        }
        catch (Exception ex)
        {
            // Payment/grant already succeeded — an email failure must not affect it.
            _logger.LogWarning(ex, "Failed to send purchase receipt for order {OrderCode}", order.OrderCode);
        }
    }

    private async Task<string> GenerateUniqueOrderCodeAsync(string provider)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var code = PaymentOrder.GenerateOrderCode(provider);
            if (!await _db.PaymentOrders.AnyAsync(o => o.OrderCode == code))
                return code;
        }
        throw new InvalidOperationException("Could not generate a unique order code.");
    }
}
