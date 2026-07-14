using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IPolarService
{
    /// <summary>Create a Polar checkout session for the given cart and return its hosted URL.</summary>
    Task<string> CreateCheckoutUrlAsync(int userId, List<CheckoutLineDto> lines);

    /// <summary>Verify a webhook signature and, if valid & paid, grant entitlements. Returns true if handled.</summary>
    Task<bool> HandleWebhookAsync(string rawBody, string? webhookId, string? webhookTimestamp, string? webhookSignature);
}
