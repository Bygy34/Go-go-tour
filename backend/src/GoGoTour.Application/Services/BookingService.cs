using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using GoGoTour.Domain.Entities;

namespace GoGoTour.Application.Services
{
    public class BookingService
    {
        private readonly IGenericRepository<BookingRequest> _bookingRepository;
        private readonly IGenericRepository<Tour> _tourRepository;

        public BookingService(
            IGenericRepository<BookingRequest> bookingRepository,
            IGenericRepository<Tour> tourRepository)
        {
            _bookingRepository = bookingRepository;
            _tourRepository = tourRepository;
        }

        public async Task<bool> CreateAsync(CreateBookingDto dto, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tour = await _tourRepository.GetByIdAsync(dto.TourId, cancellationToken);
            if (tour == null || !tour.IsActive)
            {
                return false;
            }

            await _bookingRepository.AddAsync(new BookingRequest
            {
                TourId = dto.TourId,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Message = dto.Message,
                CreatedAtUtc = System.DateTime.UtcNow
            }, cancellationToken);

            return true;
        }

        public async Task<IReadOnlyList<BookingListItemDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var bookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var tours = await _tourRepository.GetAllAsync(cancellationToken);

            return bookings
                .OrderByDescending(b => b.CreatedAtUtc)
                .Select(b => new BookingListItemDto(
                    b.Id,
                    tours.FirstOrDefault(t => t.Id == b.TourId) != null ? tours.FirstOrDefault(t => t.Id == b.TourId).Title : "Неизвестный тур",
                    b.FullName,
                    b.Email,
                    b.Phone,
                    b.Message,
                    b.CreatedAtUtc))
                .ToList();
        }
    }
}
