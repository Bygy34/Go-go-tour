using GoGoTour.Application.DTOs;

namespace GoGoTour.Api.Models;

public class AdminPageVm
{
    public List<TourDetailDto> Tours { get; set; } = new List<TourDetailDto>();
    public List<BookingAdminDto> Bookings { get; set; } = new List<BookingAdminDto>();
    public TourFormVm TourForm { get; set; } = new();
    public string? Message { get; set; }
}

public class TourFormVm
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; } = true;
}
