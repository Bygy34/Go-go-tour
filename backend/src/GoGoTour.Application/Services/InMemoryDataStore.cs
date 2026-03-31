using GoGoTour.Domain.Entities;

namespace GoGoTour.Application.Services;

public class InMemoryDataStore
{
    private readonly object _sync = new();
    private int _tourId = 3;
    private int _bookingId = 1;

    public List<Tour> Tours { get; } = new()
    {
        new Tour
        {
            Id = 1,
            Title = "Италия: Рим и Флоренция",
            Country = "Италия",
            Description = "Классический экскурсионный тур по Италии.",
            Price = 1299,
            DurationDays = 7,
            IsActive = true
        },
        new Tour
        {
            Id = 2,
            Title = "Испания: Барселона и море",
            Country = "Испания",
            Description = "Город + пляжный отдых.",
            Price = 1090,
            DurationDays = 6,
            IsActive = true
        }
    };

    public List<BookingRequest> Bookings { get; } = new();

    public int NextTourId()
    {
        lock (_sync)
        {
            return _tourId++;
        }
    }

    public int NextBookingId()
    {
        lock (_sync)
        {
            return _bookingId++;
        }
    }
}
