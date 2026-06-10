namespace backend.Models;

/// <summary>
/// Represents an airport supported by the SkyRoute platform.
/// </summary>
public class Airport
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string City { get; init; }
    public required string Country { get; init; }
}
