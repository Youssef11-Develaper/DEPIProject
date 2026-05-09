using MediatR;
using Mawidy.Application.DTOs.Courts;
using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Application.Features.Courts.Queries;

public record GetCourtsQuery(string? FilterType, string? SearchQuery) : IRequest<List<CourtDto>>;

public class GetCourtsQueryHandler : IRequestHandler<GetCourtsQuery, List<CourtDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCourtsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourtDto>> Handle(GetCourtsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Courts.AsQueryable();

        if (!string.IsNullOrEmpty(request.FilterType) && request.FilterType != "الكل")
        {
            // E.g., mapping Arabic to Enum, simplistic mapping for demonstration:
            // In real scenario, better mapping or enum string conversion
        }

        if (!string.IsNullOrEmpty(request.SearchQuery))
        {
            query = query.Where(c => c.Name.Contains(request.SearchQuery) || c.Address.Contains(request.SearchQuery));
        }

        var courts = await query.ToListAsync(cancellationToken);

        return courts.Select(c => new CourtDto
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type.ToString(),
            Address = c.Address,
            Phone = c.Phone,
            TotalRooms = c.TotalRooms,
            Status = "open", // Mock status
            DistanceKm = 4.0, // Mock distance
            QueueCount = 10,
            SessionsToday = 50,
            WaitTime = "~10 minutes"
        }).ToList();
    }
}
