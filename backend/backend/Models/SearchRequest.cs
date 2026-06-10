using System.ComponentModel.DataAnnotations;

namespace backend.Models;

/// <summary>
/// Incoming payload for a flight search request.
/// </summary>
public class SearchRequest
{
    [Required]
    public required string Origin { get; set; }

    [Required]
    public required string Destination { get; set; }

    [Required]
    public DateTime DepartureDate { get; set; }

    [Range(1, 9)]
    public int Passengers { get; set; }

    [Required]
    public required string CabinClass { get; set; }
}
