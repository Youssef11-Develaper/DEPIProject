using MediatR;
using Mawidy.Application.DTOs.Courts;
using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Application.Features.Courts.Queries;

public class TrackCaseQueryHandler : IRequestHandler<TrackCaseQuery, LegalCaseDto?>
{
    private readonly IApplicationDbContext _context;

    public TrackCaseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LegalCaseDto?> Handle(TrackCaseQuery request, CancellationToken cancellationToken)
    {
        // For demonstration, mock the case timeline response
        return new LegalCaseDto
        {
            Id = Guid.NewGuid(),
            CaseNumber = request.CaseNumber,
            Title = $"قضية رقم {request.CaseNumber}",
            Type = "دعوى مدنية — محكمة القاهرة الابتدائية — دائرة مدنية أولى",
            Timeline = new List<CaseTimelineEventDto>
            {
                new CaseTimelineEventDto { Status = "done", Icon = "✅", Title = "إيداع الدعوى", Date = "10 أكتوبر 2025" },
                new CaseTimelineEventDto { Status = "done", Icon = "✅", Title = "قيد القضية بالجدول", Date = "12 أكتوبر 2025" },
                new CaseTimelineEventDto { Status = "done", Icon = "✅", Title = "الجلسة الأولى — تأجيل للإعلان", Date = "5 نوفمبر 2025" },
                new CaseTimelineEventDto { Status = "active", Icon = "⚡", Title = "الجلسة القادمة — مرافعة", Date = "20 يناير 2026", Note = "الجلسة القادمة بعد 5 أيام" }
            }
        };
    }
}
