using GoGoTour.Application.DTOs;

namespace GoGoTour.Api.Models
{
    public class TourDetailsPageVm
    {
        public TourDetailsDto Tour { get; set; }
        public BookingFormVm BookingForm { get; set; }
        public string StatusMessage { get; set; }
    }
}
