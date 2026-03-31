using System.ComponentModel.DataAnnotations;

namespace GoGoTour.Api.Models;

public class BookingFormVm
{
    public int TourId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}
