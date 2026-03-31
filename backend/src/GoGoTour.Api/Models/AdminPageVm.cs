using System.Collections.Generic;
using GoGoTour.Application.DTOs;

namespace GoGoTour.Api.Models
{
    public class AdminPageVm
    {
        public AdminPageVm()
        {
            Tours = new List<TourDetailsDto>();
            Bookings = new List<BookingListItemDto>();
            TourForm = new TourFormVm();
        }

        public List<TourDetailsDto> Tours { get; set; }
        public List<BookingListItemDto> Bookings { get; set; }
        public TourFormVm TourForm { get; set; }
        public string Message { get; set; }
    }

    public class TourFormVm
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; }
    }
}
