using System.Text.RegularExpressions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/bookings")]
public partial class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all bookings, most recent first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingSummaryResponse>>> GetAll()
    {
        var bookings = await _bookingService.GetAllAsync();
        return Ok(bookings);
    }

    /// <summary>
    /// Creates a booking for the selected flight and passenger details.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] BookingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!AirportRegistry.IsValidCode(request.Origin) || !AirportRegistry.IsValidCode(request.Destination))
        {
            ModelState.AddModelError(nameof(request.Origin), "Origin and destination must be valid airport codes.");
        }

        ValidateDocument(request);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _bookingService.CreateAsync(request);
        return CreatedAtAction(nameof(Create), new { reference = response.BookingReference }, response);
    }

    /// <summary>
    /// Clears all persisted bookings. Intended as a development/test utility.
    /// </summary>
    [HttpDelete("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset()
    {
        await _bookingService.ResetAsync();
        return Ok(new { message = "Bookings cleared successfully" });
    }

    private void ValidateDocument(BookingRequest request)
    {
        var details = request.PassengerDetails;
        var documentNumber = details.DocumentNumber ?? string.Empty;

        if (string.Equals(details.DocumentType, "Passport", StringComparison.OrdinalIgnoreCase))
        {
            if (!PassportRegex().IsMatch(documentNumber))
            {
                ModelState.AddModelError(
                    "PassengerDetails.DocumentNumber",
                    "Passport number must be alphanumeric and 6-9 characters long.");
            }
        }
        else if (string.Equals(details.DocumentType, "NationalId", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(details.DocumentType, "National ID", StringComparison.OrdinalIgnoreCase))
        {
            if (!NationalIdRegex().IsMatch(documentNumber))
            {
                ModelState.AddModelError(
                    "PassengerDetails.DocumentNumber",
                    "National ID must be 7-8 alphanumeric characters.");
            }
        }
        else
        {
            ModelState.AddModelError(
                "PassengerDetails.DocumentType",
                "Document type must be Passport or NationalId.");
        }
    }

    [GeneratedRegex("^[A-Za-z0-9]{6,9}$")]
    private static partial Regex PassportRegex();

    [GeneratedRegex("^[A-Za-z0-9]{7,8}$")]
    private static partial Regex NationalIdRegex();
}
