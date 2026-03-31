using GoGoTour.Api.Models;
using GoGoTour.Application.DTOs;
using GoGoTour.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoGoTour.Api.Controllers;

public class AdminController : Controller
{
    private readonly TourService _tourService;
    private readonly BookingService _bookingService;

    public AdminController(TourService tourService, BookingService bookingService)
    {
        _tourService = tourService;
        _bookingService = bookingService;
    }

    [HttpGet("admin")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vm = await BuildPageVm(cancellationToken);
        return View(vm);
    }

    [HttpPost("admin/save-tour")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTour(TourFormVm form, CancellationToken cancellationToken)
    {
        if (form.Id.HasValue)
        {
            await _tourService.UpdateAsync(form.Id.Value, new UpsertTourDto(form.Title, form.Country, form.Description, form.Price, form.DurationDays, form.IsActive), cancellationToken);
        }
        else
        {
            await _tourService.CreateAsync(new UpsertTourDto(form.Title, form.Country, form.Description, form.Price, form.DurationDays, form.IsActive), cancellationToken);
        }

        var vm = await BuildPageVm(cancellationToken);
        vm.Message = "Тур сохранен.";
        return View("Index", vm);
    }

    [HttpGet("admin/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var vm = await BuildPageVm(cancellationToken);
        var tour = vm.Tours.FirstOrDefault(x => x.Id == id);
        if (tour is null)
        {
            vm.Message = "Тур не найден.";
            return View("Index", vm);
        }

        vm.TourForm = new TourFormVm
        {
            Id = tour.Id,
            Title = tour.Title,
            Country = tour.Country,
            Description = tour.Description,
            Price = tour.Price,
            DurationDays = tour.DurationDays,
            IsActive = true
        };

        return View("Index", vm);
    }

    private async Task<AdminPageVm> BuildPageVm(CancellationToken cancellationToken)
    {
        return new AdminPageVm
        {
            Tours = (await _tourService.GetAllForAdminAsync(cancellationToken)).ToList(),
            Bookings = (await _bookingService.GetAllForAdminAsync(cancellationToken)).ToList()
        };
    }
}
