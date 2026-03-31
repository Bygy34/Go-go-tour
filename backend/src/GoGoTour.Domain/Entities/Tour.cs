using System.Collections.Generic;

namespace GoGoTour.Domain.Entities
{
    public class Tour
    {
        public Tour()
        {
            BookingRequests = new List<BookingRequest>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; }

        public ICollection<BookingRequest> BookingRequests { get; set; }
    }
}
