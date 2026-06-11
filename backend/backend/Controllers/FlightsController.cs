using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(IFlightService flightService, ILogger<FlightsController> logger)
    {
        _flightService = flightService;
        _logger = logger;
    }

    /// <summary>
    /// Searches available flights for the requested route, date, cabin class and passenger count.
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(SearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<SearchResult> Search([FromBody] SearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!AirportRegistry.IsValidCode(request.Origin))
        {
            ModelState.AddModelError(nameof(request.Origin), "Origin must be a valid airport code.");
        }

        if (!AirportRegistry.IsValidCode(request.Destination))
        {
            ModelState.AddModelError(nameof(request.Destination), "Destination must be a valid airport code.");
        }

        if (string.Equals(request.Origin, request.Destination, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(request.Destination), "Origin and destination must be different.");
        }

        if (request.DepartureDate.Date < DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(nameof(request.DepartureDate), "Departure date cannot be in the past.");
        }

        if (!IsValidCabinClass(request.CabinClass))
        {
            ModelState.AddModelError(nameof(request.CabinClass), "Cabin class must be Economy, Business or FirstClass.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var results = _flightService.Search(request);
        _logger.LogInformation(
            "Flight search {Origin}->{Destination} on {Date}: {Matches} match(es), {Suggestions} suggestion(s).",
            request.Origin, request.Destination, request.DepartureDate.Date,
            results.Matches.Count, results.Suggestions.Count);

        return Ok(results);
    }

    private static bool IsValidCabinClass(string cabinClass) =>
        cabinClass is "Economy" or "Business" or "FirstClass";
}
