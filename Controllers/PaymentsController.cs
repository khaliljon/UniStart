using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPolarService _polar;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPolarService polar, ILogger<PaymentsController> logger)
    {
        _polar = polar;
        _logger = logger;
    }

    private int CurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) && id > 0 ? id : 0;
    }

    [HttpPost("api/payments/checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckout([FromBody] RunCheckoutDto dto)
    {
        try
        {
            var url = await _polar.CreateCheckoutUrlAsync(CurrentUserId(), dto.Lines);
            return Ok(new { url });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return StatusCode(502, new { error = ex.Message }); }
    }

    [HttpPost("api/webhooks/polar")]
    [AllowAnonymous]
    public async Task<IActionResult> PolarWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var raw = await reader.ReadToEndAsync();

        var id = Request.Headers["webhook-id"].FirstOrDefault();
        var timestamp = Request.Headers["webhook-timestamp"].FirstOrDefault();
        var signature = Request.Headers["webhook-signature"].FirstOrDefault();

        var ok = await _polar.HandleWebhookAsync(raw, id, timestamp, signature);
        return ok ? Ok() : Unauthorized();
    }
}
