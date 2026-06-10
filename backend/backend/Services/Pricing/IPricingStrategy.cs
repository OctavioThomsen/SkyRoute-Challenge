using backend.Models;

namespace backend.Services.Pricing;

/// <summary>
/// Defines how a provider derives the final price-per-person from a base fare.
/// New providers can be onboarded by adding a new implementation without modifying
/// existing pricing logic (Strategy pattern).
/// </summary>
public interface IPricingStrategy
{
    /// <summary>
    /// The provider this strategy applies to.
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Calculates the final price per person for the given base fare.
    /// </summary>
    decimal CalculatePricePerPerson(decimal baseFare);
}
