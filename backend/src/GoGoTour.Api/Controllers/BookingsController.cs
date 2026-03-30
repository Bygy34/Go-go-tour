using GoGoTour.Application.DTOs;
using GoGoTour.Application.Services;
using GoGoTour.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GoGoTour.Api.Controllers;

[Route("api/bookings")]
public class BookingsController : BaseAdminController
{
    private readonly BookingService _bookingService;

    public BookingsController(BookingService bookingService, IOptions<AdminAuthOptions> adminAuthOptions) : base(adminAuthOptions)
    {
        _bookingService = bookingService;
    }

    [HttpPost("{tourId:int}")]
    public async Task<ActionResult> Create([FromRoute] int tourId, [FromBody] CreateBookingRequestDto dto, CancellationToken cancellationToken)
    {
        var created = await _bookingService.CreateBookingAsync(tourId, dto, cancellationToken);
        return created ? Ok() : NotFound("Active tour was not found");
    }

    [HttpGet("admin")]
    public async Task<ActionResult<IReadOnlyList<BookingAdminDto>>> GetAll(CancellationToken cancellationToken)
    {
        if (!IsAuthorizedAdmin())
        {
            return Unauthorized("Admin token is invalid");
        }

        return Ok(await _bookingService.GetAllForAdminAsync(cancellationToken));
    }
}
