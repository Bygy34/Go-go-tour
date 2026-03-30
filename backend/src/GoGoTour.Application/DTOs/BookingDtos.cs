namespace GoGoTour.Application.DTOs;

public record CreateBookingRequestDto(string FullName, string Email, string Phone, string Message);

public record BookingAdminDto(
    int Id,
    int TourId,
    string TourTitle,
    string FullName,
    string Email,
    string Phone,
    string Message,
    DateTime CreatedAtUtc);
