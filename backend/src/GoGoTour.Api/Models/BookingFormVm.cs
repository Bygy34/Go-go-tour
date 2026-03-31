using System.ComponentModel.DataAnnotations;

namespace GoGoTour.Api.Models;

public class BookingFormVm
{
    public int TourId { get; set; }

    [RegularExpression(@"^\+381\d{9}$", ErrorMessage = "Phone number must start with +380 and contain 13 digits total.")]
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}
