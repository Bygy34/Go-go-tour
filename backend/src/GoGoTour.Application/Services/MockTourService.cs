using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using GoGoTour.Domain.Entities;

namespace GoGoTour.Application.Services;

public class MockTourService : ITourService
{
    private readonly InMemoryDataStore _store;

    public MockTourService(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<TourSummaryDto>> GetActiveToursAsync(CancellationToken cancellationToken = default)
    {
        var result = _store.Tours
            .Where(t => t.IsActive)
            .OrderBy(t => t.Title)
            .Select(ToSummary)
            .ToList();

        return Task.FromResult<IReadOnlyList<TourSummaryDto>>(result);
    }

    public Task<TourDetailDto?> GetTourByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tour = _store.Tours.FirstOrDefault(t => t.Id == id && t.IsActive);
        return Task.FromResult(tour is null ? null : ToDetail(tour));
    }

    public Task<IReadOnlyList<TourDetailDto>> GetAllToursForAdminAsync(CancellationToken cancellationToken = default)
    {
        var result = _store.Tours
            .OrderByDescending(t => t.Id)
            .Select(ToDetail)
            .ToList();

        return Task.FromResult<IReadOnlyList<TourDetailDto>>(result);
    }

    public Task<int> CreateTourAsync(UpsertTourDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Tour
        {
            Id = _store.NextTourId(),
            Title = dto.Title,
            Country = dto.Country,
            Description = dto.Description,
            Price = dto.Price,
            DurationDays = dto.DurationDays,
            IsActive = dto.IsActive
        };

        _store.Tours.Add(entity);
        return Task.FromResult(entity.Id);
    }

    public Task<bool> UpdateTourAsync(int id, UpsertTourDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _store.Tours.FirstOrDefault(t => t.Id == id);
        if (entity is null)
        {
            return Task.FromResult(false);
        }

        entity.Title = dto.Title;
        entity.Country = dto.Country;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.DurationDays = dto.DurationDays;
        entity.IsActive = dto.IsActive;

        return Task.FromResult(true);
    }

    private static TourSummaryDto ToSummary(Tour tour) =>
        new(tour.Id, tour.Title, tour.Country, tour.Price, tour.DurationDays);

    private static TourDetailDto ToDetail(Tour tour) =>
        new(tour.Id, tour.Title, tour.Country, tour.Description, tour.Price, tour.DurationDays);
}
