namespace backend.Services.Pricing;

/// <summary>
/// Resolves the correct <see cref="IPricingStrategy"/> for a given provider.
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculates the final price per person for a provider's base fare.
    /// </summary>
    decimal CalculatePricePerPerson(string provider, decimal baseFare);
}

/// <inheritdoc />
public class PricingService : IPricingService
{
    private readonly Dictionary<string, IPricingStrategy> _strategies;

    public PricingService(IEnumerable<IPricingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Provider, StringComparer.OrdinalIgnoreCase);
    }

    public decimal CalculatePricePerPerson(string provider, decimal baseFare)
    {
        if (!_strategies.TryGetValue(provider, out var strategy))
        {
            throw new InvalidOperationException($"No pricing strategy registered for provider '{provider}'.");
        }

        return strategy.CalculatePricePerPerson(baseFare);
    }
}
