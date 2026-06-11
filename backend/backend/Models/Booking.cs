namespace backend.Models;

/// <summary>
/// A booking persisted to the bookings data file.
/// </summary>
public class Booking
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
    public required List<PassengerDetails> PassengerDetailsList { get; set; }
    public DateTime CreatedAt { get; set; }
}
