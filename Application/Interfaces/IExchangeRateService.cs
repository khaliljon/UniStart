namespace UniStart.Application.Interfaces;

public interface IExchangeRateService
{
    Task<decimal> GetUsdToKztAsync(CancellationToken ct = default);

    Task RefreshAsync();
}
