namespace UniStart.Application.DTOs;

public record PricingDto(
    decimal MockPrice,
    decimal MaterialPrice,
    string Currency);

public class UpdatePricingDto
{
    public decimal MockPrice { get; set; }
    public decimal MaterialPrice { get; set; }
    public string Currency { get; set; } = "KZT";
}
