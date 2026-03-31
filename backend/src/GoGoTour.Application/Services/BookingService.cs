using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using GoGoTour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoGoTour.Application.Services;

public class BookingService : IBookingService
{
    private readonly IGoGoTourDbContext _dbContext;

    public BookingService(IGoGoTourDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateBookingAsync(int tourId, CreateBookingRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tourExists = await _dbContext.Tours.AnyAsync(t => t.Id == tourId && t.IsActive, cancellationToken);
        if (!tourExists)
        {
            return false;
        }

        var booking = new BookingRequest
        {
            TourId = tourId,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Message = dto.Message,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.BookingRequests.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<BookingAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.BookingRequests
            .Include(b => b.Tour)
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => new BookingAdminDto(
                b.Id,
                b.TourId,
                b.Tour != null ? b.Tour.Title : "(Удаленный тур)",
                b.FullName,
                b.Email,
                b.Phone,
                b.Message,
                b.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
