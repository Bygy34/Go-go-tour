using System;

namespace GoGoTour.Domain.Entities
{
    public class BookingRequest
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Tour Tour { get; set; }
    }
}
