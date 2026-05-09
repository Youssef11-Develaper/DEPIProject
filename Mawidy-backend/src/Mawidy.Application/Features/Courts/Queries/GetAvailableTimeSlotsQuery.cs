using MediatR;

namespace Mawidy.Application.Features.Courts.Queries;

public record GetAvailableTimeSlotsQuery(Guid CourtId, Guid DepartmentId, DateTime Date) : IRequest<List<TimeSpan>>;

using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public class GetAvailableTimeSlotsQueryHandler : IRequestHandler<GetAvailableTimeSlotsQuery, List<TimeSpan>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableTimeSlotsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TimeSpan>> Handle(GetAvailableTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        // Mock available timeslots based on court operating hours (9:00 AM to 2:30 PM)
        return new List<TimeSpan>
        {
            new TimeSpan(10, 30, 0),
            new TimeSpan(11, 0, 0),
            new TimeSpan(11, 30, 0),
            new TimeSpan(12, 30, 0),
            new TimeSpan(13, 0, 0),
            new TimeSpan(13, 30, 0),
            new TimeSpan(14, 30, 0)
        };
    }
}
