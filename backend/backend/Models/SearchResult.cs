namespace backend.Models;

/// <summary>
/// The outcome of a flight search: exact matches for the requested route, date and cabin,
/// plus suggestions for the same route (origin and destination) that differ in date or cabin.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Flights that match origin, destination, departure date and cabin class exactly.
    /// </summary>
    public List<SearchResponse> Matches { get; set; } = new();

    /// <summary>
    /// Alternative flights for the same origin and destination that differ in date or cabin.
    /// </summary>
    public List<SearchResponse> Suggestions { get; set; } = new();
}
