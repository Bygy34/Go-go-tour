using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using GoGoTour.Domain.Entities;

namespace GoGoTour.Application.Services;

public class MockBookingService : IBookingService
{
    private readonly InMemoryDataStore _store;

    public MockBookingService(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<bool> CreateBookingAsync(int tourId, CreateBookingRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tourExists = _store.Tours.Any(t => t.Id == tourId && t.IsActive);
        if (!tourExists)
        {
            return Task.FromResult(false);
        }

        var booking = new BookingRequest
        {
            Id = _store.NextBookingId(),
            TourId = tourId,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Message = dto.Message,
            CreatedAtUtc = DateTime.UtcNow
        };

        _store.Bookings.Add(booking);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<BookingAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default)
    {
        var result = _store.Bookings
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => new BookingAdminDto(
                b.Id,
                b.TourId,
                _store.Tours.FirstOrDefault(t => t.Id == b.TourId)?.Title ?? "(Удаленный тур)",
                b.FullName,
                b.Email,
                b.Phone,
                b.Message,
                b.CreatedAtUtc))
            .ToList();

        return Task.FromResult<IReadOnlyList<BookingAdminDto>>(result);
    }
}
