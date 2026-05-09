using Microsoft.EntityFrameworkCore;
using Mawidy.Application.Interfaces;
using Mawidy.Domain.Entities;

namespace Mawidy.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Court> Courts { get; set; } = null!;
    public DbSet<CourtDepartment> CourtDepartments { get; set; } = null!;
    public DbSet<CourtService> CourtServices { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<LegalCase> LegalCases { get; set; } = null!;
    public DbSet<CaseTimelineEvent> CaseTimelineEvents { get; set; } = null!;
    public DbSet<QueueTicket> QueueTickets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filters for soft delete
        modelBuilder.Entity<Court>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CourtDepartment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CourtService>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Booking>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LegalCase>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CaseTimelineEvent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<QueueTicket>().HasQueryFilter(e => !e.IsDeleted);

        // Concurrency token
        modelBuilder.Entity<Court>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<CourtDepartment>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<CourtService>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<Booking>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<LegalCase>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<CaseTimelineEvent>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<QueueTicket>().Property(e => e.RowVersion).IsRowVersion();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
