namespace GoGoTour.Application.DTOs;

public record CreateBookingDto(int TourId, string FullName, string Email, string Phone, string Message);

public record BookingListItemDto(int Id, string TourTitle, string FullName, string Email, string Phone, string Message, DateTime CreatedAtUtc);
