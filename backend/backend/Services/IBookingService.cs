using backend.Models;

namespace backend.Services;

/// <summary>
/// Creates bookings, generating a unique booking reference and persisting the result.
/// </summary>
public interface IBookingService
{
    Task<BookingResponse> CreateAsync(BookingRequest request);
    Task ResetAsync();
}
