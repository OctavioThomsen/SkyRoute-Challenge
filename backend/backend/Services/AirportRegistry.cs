using backend.Models;

namespace backend.Services;

/// <summary>
/// Provides the fixed set of airports supported by SkyRoute and helpers
/// for route classification (domestic vs international).
/// </summary>
public static class AirportRegistry
{
    public static readonly IReadOnlyList<Airport> Airports = new List<Airport>
    {
        new() { Code = "ATL", Name = "Hartsfield-Jackson International", City = "Atlanta, Georgia", Country = "USA" },
        new() { Code = "JFK", Name = "John F. Kennedy International", City = "New York, New York", Country = "USA" },
        new() { Code = "MIA", Name = "Miami International", City = "Miami, Florida", Country = "USA" },
        new() { Code = "LAX", Name = "Los Angeles International", City = "Los Angeles, California", Country = "USA" },
        new() { Code = "AEP", Name = "Aeroparque Jorge Newbery", City = "Buenos Aires", Country = "Argentina" },
        new() { Code = "EZE", Name = "Ministro Pistarini International", City = "Buenos Aires", Country = "Argentina" }
    };

    private static readonly Dictionary<string, Airport> ByCode =
        Airports.ToDictionary(a => a.Code, StringComparer.OrdinalIgnoreCase);

    public static bool IsValidCode(string code) => ByCode.ContainsKey(code);

    public static Airport? Get(string code) => ByCode.GetValueOrDefault(code);

    /// <summary>
    /// A route is international when its two endpoints are in different countries.
    /// </summary>
    public static bool IsInternational(string origin, string destination)
    {
        var from = Get(origin);
        var to = Get(destination);
        if (from is null || to is null)
        {
            return false;
        }

        return !string.Equals(from.Country, to.Country, StringComparison.OrdinalIgnoreCase);
    }
}
