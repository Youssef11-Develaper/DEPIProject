using MediatR;
using Mawidy.Application.DTOs.Courts;

namespace Mawidy.Application.Features.Courts.Queries;

public record GetCourtStatisticsQuery(Guid? CourtId) : IRequest<CourtStatisticsDto>;

using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public class GetCourtStatisticsQueryHandler : IRequestHandler<GetCourtStatisticsQuery, CourtStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetCourtStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourtStatisticsDto> Handle(GetCourtStatisticsQuery request, CancellationToken cancellationToken)
    {
        // Mock data logic for dashboard
        return new CourtStatisticsDto
        {
            SessionsToday = 128,
            SessionsChangePercentage = "8%",
            SessionsIsUp = true,
            JudgmentsIssued = 34,
            JudgmentsChangePercentage = "5%",
            JudgmentsIsUp = true,
            PostponedCases = 17,
            PostponedChangePercentage = "2%",
            PostponedIsUp = false,
            UpcomingSessions = new List<UpcomingSessionDto>
            {
                new UpcomingSessionDto { DateString = "الأحد 20 يناير", SessionsCount = 128, Status = "متاح" },
                new UpcomingSessionDto { DateString = "الاثنين 21 يناير", SessionsCount = 95, Status = "متاح" },
                new UpcomingSessionDto { DateString = "الثلاثاء 22 يناير", SessionsCount = 142, Status = "مزدحم" }
            },
            NearestCourts = new List<NearestCourtDto>
            {
                new NearestCourtDto { Name = "محكمة القاهرة الابتدائية", DistanceString = "4.0 كم", Icon = "🏛️", Status = "متاح" },
                new NearestCourtDto { Name = "محكمة الأسرة — العباسية", DistanceString = "3.5 كم", Icon = "👨‍👩‍👧", Status = "مزدحم" }
            }
        };
    }
}
