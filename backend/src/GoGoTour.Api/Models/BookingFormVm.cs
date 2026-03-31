using System.ComponentModel.DataAnnotations;

namespace GoGoTour.Api.Models
{
    public class BookingFormVm
    {
        public int TourId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
