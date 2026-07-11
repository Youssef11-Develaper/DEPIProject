using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mawidy.Application.DTOs.Courts;
using Mawidy.Application.Features.Courts.Queries;
using Mawidy.Application.Features.Courts.Commands;

namespace Mawidy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("slots")]
    public async Task<ActionResult<List<TimeSpan>>> GetAvailableSlots([FromQuery] Guid courtId, [FromQuery] Guid departmentId, [FromQuery] DateTime date)
    {
        var result = await _mediator.Send(new GetAvailableTimeSlotsQuery(courtId, departmentId, date));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<BookingResultDto>> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdStr, out var userGuid))
        {
            command.UserId = userGuid;
        }
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
