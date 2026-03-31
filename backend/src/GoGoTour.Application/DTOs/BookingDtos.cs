using System;

namespace GoGoTour.Application.DTOs
{
    public class CreateBookingDto
    {
        public CreateBookingDto() { }

        public CreateBookingDto(int tourId, string fullName, string email, string phone, string message)
        {
            TourId = tourId;
            FullName = fullName;
            Email = email;
            Phone = phone;
            Message = message;
        }

        public int TourId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
    }

    public class BookingListItemDto
    {
        public BookingListItemDto() { }

        public BookingListItemDto(int id, string tourTitle, string fullName, string email, string phone, string message, DateTime createdAtUtc)
        {
            Id = id;
            TourTitle = tourTitle;
            FullName = fullName;
            Email = email;
            Phone = phone;
            Message = message;
            CreatedAtUtc = createdAtUtc;
        }

        public int Id { get; set; }
        public string TourTitle { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
