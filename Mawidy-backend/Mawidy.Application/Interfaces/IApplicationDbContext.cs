using Microsoft.EntityFrameworkCore;
using Mawidy.Domain.Entities;

namespace Mawidy.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Court> Courts { get; set; }
    DbSet<CourtDepartment> CourtDepartments { get; set; }
    DbSet<CourtService> CourtServices { get; set; }
    DbSet<Booking> Bookings { get; set; }
    DbSet<LegalCase> LegalCases { get; set; }
    DbSet<CaseTimelineEvent> CaseTimelineEvents { get; set; }
    DbSet<QueueTicket> QueueTickets { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
