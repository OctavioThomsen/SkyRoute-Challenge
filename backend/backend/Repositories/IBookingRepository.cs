using backend.Models;

namespace backend.Repositories;

/// <summary>
/// Provides persistent storage for bookings.
/// </summary>
public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
    Task AddAsync(Booking booking);
    Task ClearAsync();
    Task<bool> ReferenceExistsAsync(string bookingReference);
}
