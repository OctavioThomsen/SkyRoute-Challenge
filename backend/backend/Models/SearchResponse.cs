namespace backend.Models;

/// <summary>
/// A single priced flight option returned from a search.
/// </summary>
public class SearchResponse
{
    public required string FlightNumber { get; set; }
    public required string Provider { get; set; }
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }
    public required string CabinClass { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsInternational { get; set; }
}
