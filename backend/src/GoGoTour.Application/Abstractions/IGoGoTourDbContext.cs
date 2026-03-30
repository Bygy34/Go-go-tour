using GoGoTour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoGoTour.Application.Abstractions;

public interface IGoGoTourDbContext
{
    DbSet<Tour> Tours { get; }
    DbSet<BookingRequest> BookingRequests { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
