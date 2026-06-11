using System.ComponentModel.DataAnnotations;

namespace backend.Models;

/// <summary>
/// Passenger details captured during the booking flow.
/// </summary>
public class PassengerDetails
{
    [Required]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string DocumentNumber { get; set; }

    [Required]
    public required string DocumentType { get; set; }
}

/// <summary>
/// Incoming payload for creating a booking.
/// </summary>
public class BookingRequest
{
    [Required]
    public required string FlightNumber { get; set; }

    [Required]
    public required string Provider { get; set; }

    [Required]
    public required string Origin { get; set; }

    [Required]
    public required string Destination { get; set; }

    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }

    [Required]
    public required string CabinClass { get; set; }

    public decimal PricePerPerson { get; set; }
    public decimal TotalPrice { get; set; }

    [Range(1, 9)]
    public int Passengers { get; set; }

    [Required]
    [MinLength(1)]
    public required List<PassengerDetails> PassengerDetailsList { get; set; }
}
