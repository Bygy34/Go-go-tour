using GoGoTour.Application.DTOs;

namespace GoGoTour.Api.Models;

public class TourDetailsPageVm
{
    public TourDetailsDto Tour { get; set; } = default!;
    public BookingFormVm BookingForm { get; set; } = new();
    public string? StatusMessage { get; set; }
}
