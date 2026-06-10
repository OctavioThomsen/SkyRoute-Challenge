namespace backend.Models;

/// <summary>
/// Represents the base fare for a specific cabin class of a flight, as stored in the
/// provider data files before any pricing strategy is applied.
/// </summary>
public class CabinFare
{
    public required string Class { get; set; }
    public decimal BaseFare { get; set; }
}
