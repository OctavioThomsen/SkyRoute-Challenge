namespace backend.Services.Pricing;

/// <summary>
/// GlobalAir pricing: applies a 15% fuel surcharge to the base fare.
/// </summary>
public class GlobalAirPricingStrategy : IPricingStrategy
{
    public string Provider => "GlobalAir";

    public decimal CalculatePricePerPerson(decimal baseFare)
    {
        return Math.Round(baseFare * 1.15m, 2);
    }
}
