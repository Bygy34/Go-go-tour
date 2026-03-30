using GoGoTour.Domain.Entities;

namespace GoGoTour.Infrastructure.Mocking;

public class InMemoryStore
{
    public List<Tour> Tours { get; } =
    [
        new Tour
        {
            Id = 1,
            Title = "Каппадокия на выходные",
            Country = "Турция",
            Description = "Полет на шарах, отель в скале и экскурсии по долинам.",
            Price = 499,
            DurationDays = 3,
            IsActive = true
        },
        new Tour
        {
            Id = 2,
            Title = "Северная Италия",
            Country = "Италия",
            Description = "Милан, Комо и гастрономический тур.",
            Price = 1199,
            DurationDays = 6,
            IsActive = true
        }
    ];

    public List<BookingRequest> BookingRequests { get; } = [];
}
