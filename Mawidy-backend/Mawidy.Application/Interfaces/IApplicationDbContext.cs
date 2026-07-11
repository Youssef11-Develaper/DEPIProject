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

    // Banks
    DbSet<Mawidy.Domain.Entities.Banks.Service> BankServices { get; set; }

    // Hospitals
    DbSet<Mawidy.Domain.Entities.Hospitals.Hospitals> Hospitals { get; set; }
    DbSet<Mawidy.Domain.Entities.Hospitals.Beds> HospitalBeds { get; set; }
    DbSet<Mawidy.Domain.Entities.Hospitals.BedTypes> HospitalBedTypes { get; set; }
    DbSet<Mawidy.Domain.Entities.Hospitals.Reservations> HospitalReservations { get; set; }
    DbSet<Mawidy.Domain.Entities.Hospitals.BlockedPhones> HospitalBlockedPhones { get; set; }
    DbSet<Mawidy.Domain.Entities.Hospitals.Reports> HospitalReports { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
