namespace backend.Models;

/// <summary>
/// A booking summary returned in the list of all bookings.
/// Document numbers are intentionally excluded from this projection.
/// </summary>
public class BookingSummaryResponse
{
    public required string BookingReference { get; set; }
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
    public int Passengers { get; set; }
    public required string PassengerName { get; set; }
    public DateTime CreatedAt { get; set; }
}
