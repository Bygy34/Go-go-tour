namespace GoGoTour.Application.DTOs;

public record TourSummaryDto(int Id, string Title, string Country, decimal Price, int DurationDays);

public record TourDetailDto(int Id, string Title, string Country, string Description, decimal Price, int DurationDays);

public record UpsertTourDto(string Title, string Country, string Description, decimal Price, int DurationDays, bool IsActive);
