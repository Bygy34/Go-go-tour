using GoGoTour.Api.Models;
using GoGoTour.Application.DTOs;
using GoGoTour.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoGoTour.Api.Controllers;

public class ToursController : Controller
{
    private readonly TourService _tourService;
    private readonly BookingService _bookingService;

    public ToursController(TourService tourService, BookingService bookingService)
    {
        _tourService = tourService;
        _bookingService = bookingService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tours = await _tourService.GetActiveToursAsync(cancellationToken);
        return View(tours);
    }

    [HttpGet("tours/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var tour = await _tourService.GetDetailsAsync(id, cancellationToken);
        if (tour is null)
        {
            return NotFound();
        }

        var model = new TourDetailsPageVm
        {
            Tour = tour,
            BookingForm = new BookingFormVm { TourId = id }
        };

        return View(model);
    }

    [HttpPost("tours/{id:int}/book")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int id, BookingFormVm form, CancellationToken cancellationToken)
    {
        var tour = await _tourService.GetDetailsAsync(id, cancellationToken);
        if (tour is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Details", new TourDetailsPageVm { Tour = tour, BookingForm = form, StatusMessage = "Проверьте форму." });
        }

        var created = await _bookingService.CreateAsync(
            new CreateBookingDto(id, form.FullName, form.Email, form.Phone, form.Message),
            cancellationToken);

        var vm = new TourDetailsPageVm
        {
            Tour = tour,
            BookingForm = new BookingFormVm { TourId = id },
            StatusMessage = created ? "Заявка отправлена." : "Не удалось отправить заявку."
        };

        return View("Details", vm);
    }
}
