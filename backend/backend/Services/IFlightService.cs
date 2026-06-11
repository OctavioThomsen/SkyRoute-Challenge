using backend.Models;

namespace backend.Services;

/// <summary>
/// Searches flights across all providers and applies provider-specific pricing.
/// </summary>
public interface IFlightService
{
    SearchResult Search(SearchRequest request);
}
