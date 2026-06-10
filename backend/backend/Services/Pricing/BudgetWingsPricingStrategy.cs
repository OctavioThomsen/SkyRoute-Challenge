namespace backend.Services.Pricing;

/// <summary>
/// BudgetWings pricing: applies a 10% discount to the base fare with a $29.99 floor.
/// </summary>
public class BudgetWingsPricingStrategy : IPricingStrategy
{
    private const decimal MinimumFare = 29.99m;

    public string Provider => "BudgetWings";

    public decimal CalculatePricePerPerson(decimal baseFare)
    {
        return Math.Round(Math.Max(baseFare * 0.90m, MinimumFare), 2);
    }
}
