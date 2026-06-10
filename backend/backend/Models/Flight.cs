namespace backend.Models;

/// <summary>
/// Represents a raw flight loaded from a provider data file.
/// </summary>
public class Flight
{
    public required string FlightNumber { get; set; }
    public required string Provider { get; set; }
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }
    public List<CabinFare> CabinClasses { get; set; } = new();
}
