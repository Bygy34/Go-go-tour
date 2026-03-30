namespace GoGoTour.Application.DTOs;

public record TourCardDto(int Id, string Title, string Country, decimal Price, int DurationDays);

public record TourDetailsDto(int Id, string Title, string Country, string Description, decimal Price, int DurationDays);

public record UpsertTourDto(string Title, string Country, string Description, decimal Price, int DurationDays, bool IsActive);
