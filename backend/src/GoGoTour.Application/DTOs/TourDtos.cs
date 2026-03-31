namespace GoGoTour.Application.DTOs
{
    public class TourCardDto
    {
        public TourCardDto() { }

        public TourCardDto(int id, string title, string country, decimal price, int durationDays)
        {
            Id = id;
            Title = title;
            Country = country;
            Price = price;
            DurationDays = durationDays;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Country { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
    }

    public class TourDetailsDto
    {
        public TourDetailsDto() { }

        public TourDetailsDto(int id, string title, string country, string description, decimal price, int durationDays)
        {
            Id = id;
            Title = title;
            Country = country;
            Description = description;
            Price = price;
            DurationDays = durationDays;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
    }

    public class UpsertTourDto
    {
        public UpsertTourDto() { }

        public UpsertTourDto(string title, string country, string description, decimal price, int durationDays, bool isActive)
        {
            Title = title;
            Country = country;
            Description = description;
            Price = price;
            DurationDays = durationDays;
            IsActive = isActive;
        }

        public string Title { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; }
    }
}
