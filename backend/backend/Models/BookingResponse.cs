namespace backend.Models;

/// <summary>
/// Response returned to the client after a booking is successfully created.
/// </summary>
public class BookingResponse
{
    public required string BookingReference { get; set; }
    public required string FlightNumber { get; set; }
    public required string PassengerName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
