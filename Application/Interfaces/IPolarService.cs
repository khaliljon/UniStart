using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IPolarService
{
    Task<PolarCheckoutResponse> CreateCheckoutUrlAsync(int userId, List<CheckoutLineDto> lines);

    Task<bool> HandleWebhookAsync(string rawBody, string? webhookId, string? webhookTimestamp, string? webhookSignature);
}
