using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
public class AdminPaymentsController : ControllerBase
{
    private readonly UniStartDbContext _db;
    private readonly IPaymentOrderService _orders;
    private readonly IKaspiPaymentEvidenceService _evidence;
    private readonly ILogger<AdminPaymentsController> _logger;

    public AdminPaymentsController(UniStartDbContext db, IPaymentOrderService orders, IKaspiPaymentEvidenceService evidence, ILogger<AdminPaymentsController> logger)
    {
        _db = db;
        _orders = orders;
        _evidence = evidence;
        _logger = logger;
    }

    /// <summary>
    /// Manually confirms a Kaspi order after an admin has verified the official Kaspi register.
    /// Grants access exactly once; never trusts a frontend "paid" signal.
    /// </summary>
    [HttpPost("kaspi/{orderCode}/confirm")]
    public async Task<IActionResult> ConfirmKaspi(string orderCode, [FromBody] KaspiConfirmDto dto)
    {
        var paymentId = dto.KaspiPaymentId?.Trim();
        if (string.IsNullOrWhiteSpace(paymentId))
            return BadRequest(new { error = "Kaspi payment id is required." });

        var order = await _db.PaymentOrders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        if (order == null)
            return NotFound(new { error = "Order not found." });

        if (order.Provider != PaymentProviders.Kaspi)
            return BadRequest(new { error = "Order is not a Kaspi order." });

        if (order.Status != PaymentOrderStatuses.Pending)
            return Conflict(new { error = $"Order is already {order.Status}." });

        if (!string.Equals(order.Currency, "KZT", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Order currency is not KZT." });

        // Trusted amount must match exactly; otherwise flag for manual review, do not grant.
        if (decimal.Round(dto.PaidAmount, 2) != decimal.Round(order.Amount, 2))
        {
            _logger.LogWarning("Kaspi confirm amount mismatch for {OrderCode}: paid={Paid} expected={Expected}",
                orderCode, dto.PaidAmount, order.Amount);
            return BadRequest(new
            {
                error = "Paid amount does not match the order amount. Access not granted — verify manually.",
                expected = order.Amount,
                paid = dto.PaidAmount,
            });
        }

        var result = await _orders.ConfirmAndGrantAsync(order, paymentId, amounts: null);
        if (result == ConfirmResult.Granted)
            await _orders.SendPurchaseReceiptForOrderAsync(order); // same receipt as Polar, first grant only
        return result switch
        {
            ConfirmResult.Granted => Ok(new { orderCode = order.OrderCode, status = order.Status, paidAt = order.PaidAt }),
            ConfirmResult.AlreadyProcessed => Conflict(new { error = "Order was already confirmed." }),
            ConfirmResult.DuplicatePaymentId => Conflict(new { error = "This Kaspi payment id is already used for another order." }),
            _ => StatusCode(500, new { error = "Unexpected confirmation result." }),
        };
    }

    /// <summary>Lists Kaspi orders for admin reconciliation (defaults to Pending).</summary>
    [HttpGet("kaspi")]
    public async Task<IActionResult> ListKaspi([FromQuery] string? status = "Pending")
    {
        var query = _db.PaymentOrders.Where(o => o.Provider == PaymentProviders.Kaspi);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);

        var rows = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.OrderCode,
                o.UserId,
                UserName = o.User.Name,
                Email = o.User.Email,
                o.Amount,
                o.Currency,
                o.Status,
                o.ExternalPaymentId,
                o.CreatedAt,
                o.PaidAt,
            })
            .ToListAsync();

        return Ok(rows);
    }

    /// <summary>
    /// Discovered Kaspi payments from email notifications that need admin attention.
    /// Defaults to Matched + RequiresReview (the primary reconciliation queue).
    /// </summary>
    [HttpGet("kaspi/notifications")]
    public async Task<IActionResult> ListKaspiNotifications([FromQuery] string? status)
    {
        var statuses = string.IsNullOrWhiteSpace(status)
            ? new[] { KaspiNotificationStatuses.Matched, KaspiNotificationStatuses.RequiresReview }
            : status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var rows = await _evidence.ListAsync(statuses);
        return Ok(rows);
    }

    /// <summary>
    /// Confirms a Matched Kaspi notification and grants access via the single grant gate
    /// (PaymentOrderService.ConfirmAndGrantAsync). RequiresReview cannot be confirmed here.
    /// </summary>
    [HttpPost("kaspi/notifications/{id:int}/confirm")]
    public async Task<IActionResult> ConfirmKaspiNotification(int id)
    {
        var result = await _evidence.ConfirmAsync(id);
        return result switch
        {
            NotificationConfirmResult.Granted => Ok(new { status = "granted" }),
            NotificationConfirmResult.AlreadyProcessed => Conflict(new { error = "Notification was already processed." }),
            NotificationConfirmResult.NotFound => NotFound(new { error = "Notification not found." }),
            NotificationConfirmResult.NotMatched => BadRequest(new { error = "Only a matched notification can be confirmed. Review it manually." }),
            NotificationConfirmResult.DuplicatePaymentId => Conflict(new { error = "This Kaspi payment id is already used for another order." }),
            NotificationConfirmResult.OrderMissing => BadRequest(new { error = "Linked order is missing or incomplete." }),
            _ => StatusCode(500, new { error = "Unexpected confirmation result." }),
        };
    }

    /// <summary>
    /// Dismisses a RequiresReview Kaspi notification (marks it Rejected). Never grants access,
    /// never touches the PaymentOrder/Purchase.
    /// </summary>
    [HttpPost("kaspi/notifications/{id:int}/reject")]
    public async Task<IActionResult> RejectKaspiNotification(int id)
    {
        var result = await _evidence.RejectAsync(id);
        return result switch
        {
            NotificationRejectResult.Rejected => Ok(new { status = "rejected" }),
            NotificationRejectResult.NotFound => NotFound(new { error = "Notification not found." }),
            NotificationRejectResult.NotReviewable => BadRequest(new { error = "Only a notification requiring review can be dismissed." }),
            _ => StatusCode(500, new { error = "Unexpected result." }),
        };
    }
}
