using Microsoft.EntityFrameworkCore;
using Mawidy.Domain.Entities;
using Mawidy.Application.Interfaces;

namespace Mawidy.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Operator>          Operators           => Set<Operator>();
    public DbSet<Branch>            Branches            => Set<Branch>();
    public DbSet<Governorate>       Governorates        => Set<Governorate>();
    public DbSet<District>          Districts           => Set<District>();
    public DbSet<OperatorService>   OperatorServices    => Set<OperatorService>();
    public DbSet<ServiceDocument>   ServiceDocuments    => Set<ServiceDocument>();
    public DbSet<Appointment>       Appointments        => Set<Appointment>();
    public DbSet<VirtualQueueEntry> VirtualQueueEntries => Set<VirtualQueueEntry>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── Operator ──────────────────────────────────────────────────────────
        mb.Entity<Operator>(e =>
        {
            e.Property(o => o.Key).HasMaxLength(50).IsRequired();
            e.Property(o => o.NameAr).HasMaxLength(100).IsRequired();
            e.Property(o => o.Color).HasMaxLength(20).IsRequired();
            e.Property(o => o.BgColor).HasMaxLength(20).IsRequired();
            e.Property(o => o.Emoji).HasMaxLength(10).IsRequired();
            e.Property(o => o.Hotline).HasMaxLength(20).IsRequired();
        });

        // ── Governorate ───────────────────────────────────────────────────────
        mb.Entity<Governorate>(e =>
        {
            e.Property(g => g.NameAr).HasMaxLength(100).IsRequired();
            e.Property(g => g.NameEn).HasMaxLength(100).IsRequired();
            e.Property(g => g.Region).HasMaxLength(100).IsRequired();
            e.Property(g => g.Emoji).HasMaxLength(10).IsRequired();
        });

        // ── District ──────────────────────────────────────────────────────────
        mb.Entity<District>(e =>
        {
            e.Property(d => d.NameAr).HasMaxLength(100).IsRequired();
            e.Property(d => d.Type).HasMaxLength(20).IsRequired();
            e.HasOne(d => d.Governorate)
             .WithMany(g => g.Districts)
             .HasForeignKey(d => d.GovernorateId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Branch ────────────────────────────────────────────────────────────
        mb.Entity<Branch>(e =>
        {
            e.Property(b => b.NameAr).HasMaxLength(200).IsRequired();
            e.Property(b => b.Area).HasMaxLength(100).IsRequired();
            e.Property(b => b.Address).HasMaxLength(300).IsRequired();
            e.Property(b => b.WaitTime).HasMaxLength(20).IsRequired();

            e.HasOne(b => b.Operator)
             .WithMany(o => o.Branches)
             .HasForeignKey(b => b.OperatorId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.Governorate)
             .WithMany()
             .HasForeignKey(b => b.GovernorateId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.District)
             .WithMany(d => d.Branches)
             .HasForeignKey(b => b.DistrictId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(b => b.OperatorId);
            e.HasIndex(b => b.GovernorateId);
            e.HasIndex(b => b.DistrictId);
        });

        // ── OperatorService ───────────────────────────────────────────────────
        mb.Entity<OperatorService>(e =>
        {
            e.Property(s => s.ServiceKey).HasMaxLength(20).IsRequired();
            e.Property(s => s.Icon).HasMaxLength(10).IsRequired();
            e.Property(s => s.NameAr).HasMaxLength(100).IsRequired();
            e.Property(s => s.EstimatedTime).HasMaxLength(50).IsRequired();
            e.HasOne(s => s.Operator)
             .WithMany(o => o.Services)
             .HasForeignKey(s => s.OperatorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ServiceDocument ───────────────────────────────────────────────────
        mb.Entity<ServiceDocument>(e =>
        {
            e.Property(d => d.ServiceKey).HasMaxLength(20).IsRequired();
            e.Property(d => d.TextAr).HasMaxLength(300).IsRequired();
            e.Property(d => d.NoteAr).HasMaxLength(300);
        });

        // ── Appointment ───────────────────────────────────────────────────────
        mb.Entity<Appointment>(e =>
        {
            e.Property(a => a.ServiceKey).HasMaxLength(20).IsRequired();
            e.Property(a => a.CustomerName).HasMaxLength(100).IsRequired();
            e.Property(a => a.CustomerPhone).HasMaxLength(20).IsRequired();
            e.Property(a => a.Notes).HasMaxLength(500);
            e.HasOne(a => a.Branch)
             .WithMany(b => b.Appointments)
             .HasForeignKey(a => a.BranchId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => a.BranchId);
            e.HasIndex(a => a.AppointmentDate);
        });

        // ── VirtualQueueEntry ─────────────────────────────────────────────────
        mb.Entity<VirtualQueueEntry>(e =>
        {
            e.Property(q => q.CustomerName).HasMaxLength(100).IsRequired();
            e.Property(q => q.CustomerPhone).HasMaxLength(20).IsRequired();
            e.Property(q => q.ServiceKey).HasMaxLength(20).IsRequired();
            e.HasOne(q => q.Branch)
             .WithMany(b => b.QueueEntries)
             .HasForeignKey(q => q.BranchId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(q => q.BranchId);
        });

        // ── Seed Data ─────────────────────────────────────────────────────────
        SeedData.SeedAll(mb);
    }
}
