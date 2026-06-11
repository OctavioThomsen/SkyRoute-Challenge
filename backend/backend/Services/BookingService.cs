using System.Security.Cryptography;
using System.Text;
using backend.Models;
using backend.Repositories;

namespace backend.Services;

/// <summary>
/// Handles booking creation: builds a unique SKY-XXXXXX reference, persists the booking,
/// and returns a confirmation response.
/// </summary>
public class BookingService : IBookingService
{
    private const string ReferencePrefix = "SKY-";
    private const string ReferenceAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int ReferenceLength = 6;

    private readonly IBookingRepository _repository;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository repository, ILogger<BookingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateAsync(BookingRequest request)
    {
        var bookingReference = await GenerateUniqueReferenceAsync();

        var booking = new Booking
        {
            BookingReference = bookingReference,
            FlightNumber = request.FlightNumber,
            Provider = request.Provider,
            Origin = request.Origin,
            Destination = request.Destination,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            DurationMinutes = request.DurationMinutes,
            CabinClass = request.CabinClass,
            PricePerPerson = request.PricePerPerson,
            TotalPrice = request.TotalPrice,
            Passengers = request.Passengers,
            PassengerDetailsList = request.PassengerDetailsList,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(booking);
        _logger.LogInformation("Created booking {Reference} for flight {Flight}.", bookingReference, request.FlightNumber);

        return new BookingResponse
        {
            BookingReference = booking.BookingReference,
            FlightNumber = booking.FlightNumber,
            PassengerName = booking.PassengerDetailsList[0].FullName,
            TotalPrice = booking.TotalPrice,
            CreatedAt = booking.CreatedAt
        };
    }

    public Task ResetAsync()
    {
        _logger.LogInformation("Clearing all bookings.");
        return _repository.ClearAsync();
    }

    public async Task<IReadOnlyList<BookingSummaryResponse>> GetAllAsync()
    {
        var bookings = await _repository.GetAllAsync();
        return bookings
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookingSummaryResponse
            {
                BookingReference = b.BookingReference,
                FlightNumber    = b.FlightNumber,
                Provider        = b.Provider,
                Origin          = b.Origin,
                Destination     = b.Destination,
                DepartureTime   = b.DepartureTime,
                ArrivalTime     = b.ArrivalTime,
                DurationMinutes = b.DurationMinutes,
                CabinClass      = b.CabinClass,
                PricePerPerson  = b.PricePerPerson,
                TotalPrice      = b.TotalPrice,
                Passengers      = b.Passengers,
                PassengerName   = b.PassengerDetailsList[0].FullName,
                CreatedAt       = b.CreatedAt
            })
            .ToList();
    }

    private async Task<string> GenerateUniqueReferenceAsync()
    {
        for (var attempt = 0; attempt < 25; attempt++)
        {
            var reference = GenerateReference();
            if (!await _repository.ReferenceExistsAsync(reference))
            {
                return reference;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique booking reference.");
    }

    private static string GenerateReference()
    {
        var builder = new StringBuilder(ReferencePrefix, ReferencePrefix.Length + ReferenceLength);
        for (var i = 0; i < ReferenceLength; i++)
        {
            var index = RandomNumberGenerator.GetInt32(ReferenceAlphabet.Length);
            builder.Append(ReferenceAlphabet[index]);
        }

        return builder.ToString();
    }
}
