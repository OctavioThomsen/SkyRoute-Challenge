using System.Text.Json;
using backend.Models;

namespace backend.Repositories;

/// <summary>
/// File-backed booking repository. This is the only component that reads from or writes to
/// the bookings data file. Writes are serialized with a <see cref="SemaphoreSlim"/> to keep
/// concurrent requests thread-safe.
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly string _filePath;
    private readonly ILogger<BookingRepository> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public BookingRepository(IWebHostEnvironment environment, ILogger<BookingRepository> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "bookings.json");
    }

    public async Task<IReadOnlyList<Booking>> GetAllAsync()
    {
        await _gate.WaitAsync();
        try
        {
            return await ReadAllUnlockedAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AddAsync(Booking booking)
    {
        await _gate.WaitAsync();
        try
        {
            var bookings = (await ReadAllUnlockedAsync()).ToList();
            bookings.Add(booking);
            await WriteAllUnlockedAsync(bookings);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _gate.WaitAsync();
        try
        {
            await WriteAllUnlockedAsync(new List<Booking>());
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> ReferenceExistsAsync(string bookingReference)
    {
        await _gate.WaitAsync();
        try
        {
            var bookings = await ReadAllUnlockedAsync();
            return bookings.Any(b => string.Equals(b.BookingReference, bookingReference, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<Booking>> ReadAllUnlockedAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Booking>();
        }

        await using var stream = File.OpenRead(_filePath);
        var bookings = await JsonSerializer.DeserializeAsync<List<Booking>>(stream, SerializerOptions);
        return bookings ?? new List<Booking>();
    }

    private async Task WriteAllUnlockedAsync(List<Booking> bookings)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, bookings, SerializerOptions);
        _logger.LogInformation("Persisted {Count} booking(s) to store.", bookings.Count);
    }
}
