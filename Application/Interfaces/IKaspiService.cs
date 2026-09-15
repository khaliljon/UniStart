using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IKaspiService
{
    /// <summary>
    /// Creates a Pending Kaspi order (trusted server price) and returns the order code plus the
    /// direct Kaspi payment URL. Opening the URL is NOT a payment confirmation.
    /// </summary>
    Task<KaspiCheckoutResponse> CreateCheckoutAsync(int userId, List<CheckoutLineDto> lines);
}
