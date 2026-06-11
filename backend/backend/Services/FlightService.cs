using System.Text.Json;
using backend.Models;
using backend.Services.Pricing;

namespace backend.Services;

/// <summary>
/// Loads provider flight data from JSON at startup, caches it in memory, and answers
/// search queries. Exact matches require origin, destination, departure date and cabin to
/// match; flights on the same route and cabin with a different date are returned as suggestions.
/// </summary>
public class FlightService : IFlightService
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<FlightService> _logger;
    private readonly IReadOnlyList<Flight> _flights;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FlightService(IWebHostEnvironment environment, IPricingService pricingService, ILogger<FlightService> logger)
    {
        _pricingService = pricingService;
        _logger = logger;

        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        var flights = new List<Flight>();
        flights.AddRange(LoadProviderFile(Path.Combine(dataDirectory, "globalair.json")));
        flights.AddRange(LoadProviderFile(Path.Combine(dataDirectory, "budgetwings.json")));
        flights.AddRange(LoadProviderFile(Path.Combine(dataDirectory, "arcticair.json")));
        _flights = flights;

        _logger.LogInformation("Loaded {Count} flights from provider data files.", _flights.Count);
    }

    public SearchResult Search(SearchRequest request)
    {
        var isInternational = AirportRegistry.IsInternational(request.Origin, request.Destination);
        var matches = new List<SearchResponse>();
        var suggestions = new List<SearchResponse>();
        var today = DateTime.UtcNow.Date;

        foreach (var flight in _flights)
        {
            // Same route only (origin + destination).
            if (!string.Equals(flight.Origin, request.Origin, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(flight.Destination, request.Destination, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Never offer flights that already departed.
            if (flight.DepartureTime.Date < today)
            {
                continue;
            }

            var isRequestedDate = flight.DepartureTime.Date == request.DepartureDate.Date;

            foreach (var cabin in flight.CabinClasses)
            {
                var isRequestedCabin = string.Equals(cabin.Class, request.CabinClass, StringComparison.OrdinalIgnoreCase);

                // Only process the requested cabin class.
                if (!isRequestedCabin)
                {
                    continue;
                }

                var response = BuildResponse(flight, cabin, request.Passengers, isInternational);

                // Exact match: requested route + date + cabin.
                if (isRequestedDate)
                {
                    matches.Add(response);
                }
                else
                {
                    // Same route + cabin, different date.
                    suggestions.Add(response);
                }
            }
        }

        var orderedSuggestions = suggestions
            .OrderBy(s => Math.Abs((s.DepartureTime.Date - request.DepartureDate.Date).Days))
            .ThenBy(s => s.TotalPrice)
            .Take(24)
            .ToList();

        return new SearchResult
        {
            Matches = matches.OrderBy(m => m.TotalPrice).ToList(),
            Suggestions = orderedSuggestions
        };
    }

    private SearchResponse BuildResponse(Flight flight, CabinFare cabin, int passengers, bool isInternational)
    {
        var pricePerPerson = _pricingService.CalculatePricePerPerson(flight.Provider, cabin.BaseFare);

        return new SearchResponse
        {
            FlightNumber = flight.FlightNumber,
            Provider = flight.Provider,
            Origin = flight.Origin,
            Destination = flight.Destination,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.DepartureTime.AddMinutes(flight.DurationMinutes),
            DurationMinutes = flight.DurationMinutes,
            CabinClass = cabin.Class,
            PricePerPerson = pricePerPerson,
            TotalPrice = Math.Round(pricePerPerson * passengers, 2),
            IsInternational = isInternational
        };
    }

    private IEnumerable<Flight> LoadProviderFile(string path)
    {
        if (!File.Exists(path))
        {
            _logger.LogWarning("Provider data file not found: {Path}", path);
            return Enumerable.Empty<Flight>();
        }

        var json = File.ReadAllText(path);
        var flights = JsonSerializer.Deserialize<List<Flight>>(json, SerializerOptions);
        return flights ?? Enumerable.Empty<Flight>();
    }
}
