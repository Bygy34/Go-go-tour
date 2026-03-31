using GoGoTour.Application.DTOs;

namespace GoGoTour.Api.Models;

public class TourDetailsPageVm
{
    public TourDetailDto Tour { get; set; } = default!;
    public BookingFormVm BookingForm { get; set; } = new();
    public string? StatusMessage { get; set; }
}
