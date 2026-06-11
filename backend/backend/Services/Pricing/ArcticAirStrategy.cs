namespace backend.Services.Pricing;

/// <summary>
/// ArcticAir pricing: base fare × 1.20, then subtract $10 loyalty discount.
/// Minimum final price: $49.99.
/// </summary>
public class ArcticAirStrategy : IPricingStrategy
{
    private const decimal Multiplier = 1.20m;
    private const decimal LoyaltyDiscount = 10.00m;
    private const decimal MinimumFare = 49.99m;

    public string Provider => "ArcticAir";

    public decimal CalculatePricePerPerson(decimal baseFare)
    {
        var price = baseFare * Multiplier - LoyaltyDiscount;
        return Math.Round(Math.Max(price, MinimumFare), 2);
    }
}

