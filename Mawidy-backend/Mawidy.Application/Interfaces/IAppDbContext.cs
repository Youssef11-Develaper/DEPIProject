using Microsoft.EntityFrameworkCore;
using Mawidy.Domain.Entities;

namespace Mawidy.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Operator>          Operators           { get; }
    DbSet<Branch>            Branches            { get; }
    DbSet<Governorate>       Governorates        { get; }
    DbSet<District>          Districts           { get; }
    DbSet<OperatorService>   OperatorServices    { get; }
    DbSet<ServiceDocument>   ServiceDocuments    { get; }
    DbSet<Appointment>       Appointments        { get; }
    DbSet<VirtualQueueEntry> VirtualQueueEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
