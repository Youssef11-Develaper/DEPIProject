using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mawidy.Application.DTOs.Courts;
using Mawidy.Application.Features.Courts.Queries;

namespace Mawidy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourtsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CourtsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourtDto>>> GetCourts([FromQuery] string? filterType, [FromQuery] string? searchQuery)
    {
        var result = await _mediator.Send(new GetCourtsQuery(filterType, searchQuery));
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<CourtStatisticsDto>> GetStats([FromQuery] Guid? courtId)
    {
        var result = await _mediator.Send(new GetCourtStatisticsQuery(courtId));
        return Ok(result);
    }

    [HttpGet("{courtId}/queue")]
    public async Task<ActionResult<LiveQueueDto>> GetLiveQueue(Guid courtId)
    {
        var result = await _mediator.Send(new GetLiveQueueQuery(courtId));
        return Ok(result);
    }
}
