using System.Text.Json;
using backend.Models;
using backend.Services.Pricing;

namespace backend.Services;

/// <summary>
/// Loads provider flight data from JSON at startup, caches it in memory, and answers
/// search queries by filtering on route/date/cabin and applying the pricing strategy
/// for each provider.
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
        _flights = flights;

        _logger.LogInformation("Loaded {Count} flights from provider data files.", _flights.Count);
    }

    public IReadOnlyList<SearchResponse> Search(SearchRequest request)
    {
        var isInternational = AirportRegistry.IsInternational(request.Origin, request.Destination);

        var results = new List<SearchResponse>();

        foreach (var flight in _flights)
        {
            if (!string.Equals(flight.Origin, request.Origin, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(flight.Destination, request.Destination, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var cabin = flight.CabinClasses
                .FirstOrDefault(c => string.Equals(c.Class, request.CabinClass, StringComparison.OrdinalIgnoreCase));

            if (cabin is null)
            {
                continue;
            }

            var pricePerPerson = _pricingService.CalculatePricePerPerson(flight.Provider, cabin.BaseFare);

            // Provider data represents a recurring daily schedule. Project the stored time-of-day
            // onto the requested departure date so searches work for any future date.
            var departureTime = request.DepartureDate.Date + flight.DepartureTime.TimeOfDay;
            var arrivalTime = departureTime.AddMinutes(flight.DurationMinutes);

            results.Add(new SearchResponse
            {
                FlightNumber = flight.FlightNumber,
                Provider = flight.Provider,
                Origin = flight.Origin,
                Destination = flight.Destination,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                DurationMinutes = flight.DurationMinutes,
                CabinClass = cabin.Class,
                PricePerPerson = pricePerPerson,
                TotalPrice = Math.Round(pricePerPerson * request.Passengers, 2),
                IsInternational = isInternational
            });
        }

        return results;
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
