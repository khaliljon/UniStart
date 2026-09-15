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
    private readonly ILogger<AdminPaymentsController> _logger;

    public AdminPaymentsController(UniStartDbContext db, IPaymentOrderService orders, ILogger<AdminPaymentsController> logger)
    {
        _db = db;
        _orders = orders;
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
}
