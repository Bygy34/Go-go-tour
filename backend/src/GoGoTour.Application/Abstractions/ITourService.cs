using GoGoTour.Application.DTOs;

namespace GoGoTour.Application.Abstractions;

public interface ITourService
{
    Task<IReadOnlyList<TourSummaryDto>> GetActiveToursAsync(CancellationToken cancellationToken = default);
    Task<TourDetailDto?> GetTourByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TourDetailDto>> GetAllToursForAdminAsync(CancellationToken cancellationToken = default);
    Task<int> CreateTourAsync(UpsertTourDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateTourAsync(int id, UpsertTourDto dto, CancellationToken cancellationToken = default);
}
