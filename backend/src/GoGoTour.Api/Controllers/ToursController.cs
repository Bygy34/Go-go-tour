using GoGoTour.Application.Abstractions;
using GoGoTour.Application.DTOs;
using GoGoTour.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GoGoTour.Api.Controllers;

[Route("api/tours")]
public class ToursController : BaseAdminController
{
    private readonly ITourService _tourService;

    public ToursController(ITourService tourService, IOptions<AdminAuthOptions> adminAuthOptions) : base(adminAuthOptions)
    {
        _tourService = tourService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TourSummaryDto>>> GetActiveTours(CancellationToken cancellationToken)
    {
        return Ok(await _tourService.GetActiveToursAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TourDetailDto>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var tour = await _tourService.GetTourByIdAsync(id, cancellationToken);
        return Ok(tour);
    }

    [HttpGet("admin")]
    public async Task<ActionResult<IReadOnlyList<TourDetailDto>>> GetAdminTours(CancellationToken cancellationToken)
    {
        if (!IsAuthorizedAdmin())
        {
            return Unauthorized("Admin token is invalid");
        }

        return Ok(await _tourService.GetAllToursForAdminAsync(cancellationToken));
    }

    [HttpPost("admin")]
    public async Task<ActionResult<int>> Create([FromBody] UpsertTourDto dto, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedAdmin())
        {
            return Unauthorized("Admin token is invalid");
        }

        var id = await _tourService.CreateTourAsync(dto, cancellationToken);
        return Ok(id);
    }

    [HttpPut("admin/{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpsertTourDto dto, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedAdmin())
        {
            return Unauthorized("Admin token is invalid");
        }

        var updated = await _tourService.UpdateTourAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}
