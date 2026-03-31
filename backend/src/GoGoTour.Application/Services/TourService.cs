using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using GoGoTour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoGoTour.Application.Services;

public class TourService : ITourService
{
    private readonly IGoGoTourDbContext _dbContext;

    public TourService(IGoGoTourDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TourSummaryDto>> GetActiveToursAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tours
            .Where(t => t.IsActive)
            .OrderBy(t => t.Title)
            .Select(t => new TourSummaryDto(t.Id, t.Title, t.Country, t.Price, t.DurationDays))
            .ToListAsync(cancellationToken);
    }

    public async Task<TourDetailDto?> GetTourByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tours
            .Where(t => t.IsActive && t.Id == id)
            .Select(t => new TourDetailDto(t.Id, t.Title, t.Country, t.Description, t.Price, t.DurationDays))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TourDetailDto>> GetAllToursForAdminAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tours
            .OrderByDescending(t => t.Id)
            .Select(t => new TourDetailDto(t.Id, t.Title, t.Country, t.Description, t.Price, t.DurationDays))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateTourAsync(UpsertTourDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Tour
        {
            Title = dto.Title,
            Country = dto.Country,
            Description = dto.Description,
            Price = dto.Price,
            DurationDays = dto.DurationDays,
            IsActive = dto.IsActive
        };

        _dbContext.Tours.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateTourAsync(int id, UpsertTourDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Tours.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Title = dto.Title;
        entity.Country = dto.Country;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.DurationDays = dto.DurationDays;
        entity.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
