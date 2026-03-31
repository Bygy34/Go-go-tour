using GoGoTour.Application.DTOs;

namespace GoGoTour.Application.Abstractions;

public interface IBookingService
{
    Task<bool> CreateBookingAsync(int tourId, CreateBookingRequestDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
}
