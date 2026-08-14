using System.Text.Json;

namespace UniStart.Application.Interfaces;

public interface ITelegramBotService
{
    bool IsConfigured { get; }

    string? WebhookSecret { get; }

    Task ProcessUpdateAsync(JsonElement update, CancellationToken ct = default);

    Task<bool> SendOperatorReplyAsync(int ticketId, string text, CancellationToken ct = default);
}
