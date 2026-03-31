namespace GoGoTour.Domain.Entities;

public class BookingRequest
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Tour? Tour { get; set; }
}
