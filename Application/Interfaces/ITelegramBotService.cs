using System.Text.Json;

namespace UniStart.Application.Interfaces;

/// <summary>Handles Telegram bot updates delivered via webhook.</summary>
public interface ITelegramBotService
{
    /// <summary>True when a bot token is configured.</summary>
    bool IsConfigured { get; }

    /// <summary>The configured webhook secret (used to validate incoming requests). Null when not set.</summary>
    string? WebhookSecret { get; }

    /// <summary>Process a single Telegram Update payload.</summary>
    Task ProcessUpdateAsync(JsonElement update, CancellationToken ct = default);

    /// <summary>Send a reply from support staff to a ticket's user. Returns true on success.</summary>
    Task<bool> SendOperatorReplyAsync(int ticketId, string text, CancellationToken ct = default);
}
