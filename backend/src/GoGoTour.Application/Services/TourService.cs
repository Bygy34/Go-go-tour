using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using GoGoTour.Domain.Entities;

namespace GoGoTour.Application.Services;

public class TourService
{
    private readonly IGenericRepository<Tour> _tourRepository;

    public TourService(IGenericRepository<Tour> tourRepository)
    {
        _tourRepository = tourRepository;
    }

    public async Task<IReadOnlyList<TourCardDto>> GetActiveToursAsync(CancellationToken cancellationToken = default)
    {
        var tours = await _tourRepository.GetAllAsync(cancellationToken);

        return tours
            .Where(t => t.IsActive)
            .OrderBy(t => t.Title)
            .Select(t => new TourCardDto(t.Id, t.Title, t.Country, t.Price, t.DurationDays))
            .ToList();
    }

    public async Task<TourDetailsDto?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var tour = await _tourRepository.GetByIdAsync(id, cancellationToken);
        if (tour is null || !tour.IsActive)
        {
            return null;
        }

        return new TourDetailsDto(tour.Id, tour.Title, tour.Country, tour.Description, tour.Price, tour.DurationDays);
    }

    public async Task<IReadOnlyList<TourDetailsDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default)
    {
        var tours = await _tourRepository.GetAllAsync(cancellationToken);
        return tours
            .OrderByDescending(x => x.Id)
            .Select(t => new TourDetailsDto(t.Id, t.Title, t.Country, t.Description, t.Price, t.DurationDays))
            .ToList();
    }

    public async Task CreateAsync(UpsertTourDto dto, CancellationToken cancellationToken = default)
    {
        await _tourRepository.AddAsync(new Tour
        {
            Title = dto.Title,
            Country = dto.Country,
            Description = dto.Description,
            Price = dto.Price,
            DurationDays = dto.DurationDays,
            IsActive = dto.IsActive
        }, cancellationToken);
    }

    public async Task<bool> UpdateAsync(int id, UpsertTourDto dto, CancellationToken cancellationToken = default)
    {
        var current = await _tourRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return false;
        }

        current.Title = dto.Title;
        current.Country = dto.Country;
        current.Description = dto.Description;
        current.Price = dto.Price;
        current.DurationDays = dto.DurationDays;
        current.IsActive = dto.IsActive;

        return await _tourRepository.UpdateAsync(current, cancellationToken);
    }
}
