using GoGoTour.Api.Models;
using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GoGoTour.Api.Controllers;

public class ToursPageController : Controller
{
    private readonly ITourService _tourService;
    private readonly IBookingService _bookingService;

    public ToursPageController(ITourService tourService, IBookingService bookingService)
    {
        _tourService = tourService;
        _bookingService = bookingService;
    }

    [HttpGet("tours")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tours = await _tourService.GetActiveToursAsync(cancellationToken);
        return View("~/Views/Tours/Index.cshtml", tours);
    }

    [HttpGet("tours/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var vm = await BuildDetailsVm(id, cancellationToken);
        if (vm is null)
        {
            return NotFound();
        }

        return View("~/Views/Tours/Details.cshtml", vm);
    }

    [HttpPost("tours/{id:int}/book")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int id, BookingFormVm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidVm = await BuildDetailsVm(id, cancellationToken, form, "Проверьте корректность данных формы.");
            return invalidVm is null ? NotFound() : View("~/Views/Tours/Details.cshtml", invalidVm);
        }

        var created = await _bookingService.CreateBookingAsync(
            id,
            new CreateBookingRequestDto(form.FullName, form.Email, form.Phone, form.Message),
            cancellationToken);

        var statusMessage = created ? "Заявка отправлена." : "Тур не найден или неактивен.";
        var vm = await BuildDetailsVm(id, cancellationToken, new BookingFormVm { TourId = id }, statusMessage);

        return vm is null ? NotFound() : View("~/Views/Tours/Details.cshtml", vm);
    }

    private async Task<TourDetailsPageVm?> BuildDetailsVm(
        int id,
        CancellationToken cancellationToken,
        BookingFormVm? form = null,
        string? statusMessage = null)
    {
        var tour = await _tourService.GetTourByIdAsync(id, cancellationToken);
        if (tour is null)
        {
            return null;
        }

        return new TourDetailsPageVm
        {
            Tour = tour,
            BookingForm = form ?? new BookingFormVm { TourId = id },
            StatusMessage = statusMessage
        };
    }
}
