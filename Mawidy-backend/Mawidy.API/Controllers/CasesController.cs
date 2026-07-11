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
public class CasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("track/{caseNumber}")]
    public async Task<ActionResult<LegalCaseDto>> TrackCase(string caseNumber)
    {
        var result = await _mediator.Send(new TrackCaseQuery(caseNumber));
        if (result == null)
            return NotFound("Case not found");
            
        return Ok(result);
    }
}
