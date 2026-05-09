using MediatR;
using Mawidy.Application.DTOs.Courts;

namespace Mawidy.Application.Features.Courts.Queries;

public record GetLiveQueueQuery(Guid CourtId) : IRequest<LiveQueueDto>;

using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public class GetLiveQueueQueryHandler : IRequestHandler<GetLiveQueueQuery, LiveQueueDto>
{
    private readonly IApplicationDbContext _context;

    public GetLiveQueueQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LiveQueueDto> Handle(GetLiveQueueQuery request, CancellationToken cancellationToken)
    {
        // Mocking queue
        return new LiveQueueDto
        {
            CurrentQueueNumber = 47,
            StatusMessage = "المحكمة تعمل بشكل طبيعي",
            WaitingList = new List<QueueTicketDto>
            {
                new QueueTicketDto { TicketNumber = 48, ServiceName = "تسليم مستندات", Status = "wait", WaitTime = "~10 دقائق" },
                new QueueTicketDto { TicketNumber = 49, ServiceName = "استخراج حكم", Status = "wait", WaitTime = "~20 دقيقة" },
                new QueueTicketDto { TicketNumber = 50, ServiceName = "توثيق عقد", Status = "wait", WaitTime = "~30 دقيقة" }
            }
        };
    }
}
