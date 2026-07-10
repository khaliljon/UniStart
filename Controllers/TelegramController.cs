using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramBotService _bot;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(ITelegramBotService bot, ILogger<TelegramController> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    /// <summary>
    /// Telegram webhook endpoint. Configure it via setWebhook with a secret token:
    /// https://api.telegram.org/bot&lt;TOKEN&gt;/setWebhook?url=https://unistart.kz/api/telegram/webhook&amp;secret_token=&lt;SECRET&gt;
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] JsonElement update, CancellationToken ct)
    {
        if (!_bot.IsConfigured) return Ok();

        // Validate the secret token Telegram sends back with each request.
        if (!string.IsNullOrEmpty(_bot.WebhookSecret))
        {
            var provided = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
            if (!string.Equals(provided, _bot.WebhookSecret, StringComparison.Ordinal))
            {
                _logger.LogWarning("Telegram webhook: invalid secret token");
                return Unauthorized();
            }
        }

        await _bot.ProcessUpdateAsync(update, ct);
        return Ok();
    }
}
