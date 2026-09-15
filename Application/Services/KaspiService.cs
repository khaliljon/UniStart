using System.Globalization;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;

namespace UniStart.Application.Services;

public class KaspiService : IKaspiService
{
    private readonly IPaymentOrderService _orders;
    private readonly string _baseUrl;

    // Official UniStart direct link. Only the amount value is substituted.
    private const string DefaultBaseUrl =
        "https://kaspi.kz/pay/CoursesUniversal?subservice_id=26319&region_id=18&started_from=share=&amount=";

    public KaspiService(IConfiguration config, IPaymentOrderService orders)
    {
        _orders = orders;
        _baseUrl = config["KASPI_PAYMENT_URL"] ?? DefaultBaseUrl;
    }

    public async Task<KaspiCheckoutResponse> CreateCheckoutAsync(int userId, List<CheckoutLineDto> lines)
    {
        // KZT is validated before the order is persisted (no stray Pending row on mismatch).
        var order = await _orders.CreateAsync(userId, PaymentProviders.Kaspi, lines, requiredCurrency: "KZT");

        return new KaspiCheckoutResponse(
            "kaspi",
            order.OrderCode,
            order.Amount,
            order.Currency,
            BuildPaymentUrl(order.Amount));
    }

    private string BuildPaymentUrl(decimal amount)
    {
        // Kaspi's amount is in whole tenge.
        var value = ((long)Math.Round(amount, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
        return _baseUrl + value;
    }
}
