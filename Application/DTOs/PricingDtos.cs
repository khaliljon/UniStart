namespace UniStart.Application.DTOs;

/// <summary>Public pricing snapshot shown on landing and in the cart.</summary>
public record PricingDto(
    decimal MockPrice,
    decimal MaterialPrice,
    string Currency);

/// <summary>Admin request to update pricing.</summary>
public class UpdatePricingDto
{
    public decimal MockPrice { get; set; }
    public decimal MaterialPrice { get; set; }
    public string Currency { get; set; } = "KZT";
}
