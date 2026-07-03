using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BranchSchedule> BranchSchedules { get; set; }
        public DbSet<BranchHoliday> BranchHolidays { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<ServiceUnavailability> ServiceUnavailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<OperatorService> OperatorServices { get; set; }
        public DbSet<ServiceDocument> ServiceDocuments { get; set; }
        public DbSet<VirtualQueueEntry> VirtualQueueEntries { get; set; }

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

            // Mappings for shared/telecom entities to match TelecomDbContext naming convention
            modelBuilder.Entity<Operator>().ToTable("Operators");
            modelBuilder.Entity<District>().ToTable("Districts");
            modelBuilder.Entity<VirtualQueueEntry>().ToTable("VirtualQueueEntries");
            modelBuilder.Entity<OperatorService>().ToTable("OperatorServices");
            modelBuilder.Entity<ServiceDocument>().ToTable("ServiceDocuments");

            modelBuilder.Entity<Rating>()
                 .HasIndex(r => r.AppointmentId)
                 .IsUnique();

            modelBuilder.Entity<Appointment>()
                 .HasOne(a => a.User)
                 .WithMany(u => u.Appointments)
                 .HasForeignKey(a => a.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Complaint>()
                 .HasOne(c => c.User)
                 .WithMany(u => u.Complaints)
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                 .HasOne(r => r.User)
                 .WithMany(u => u.Ratings)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                 .HasOne(r => r.Branch)
                 .WithMany(b => b.Ratings)
                 .HasForeignKey(r => r.BranchId)
                 .OnDelete(DeleteBehavior.Restrict);

            // Restrict Delete Behaviors on Branch to avoid multiple cascade path cycles in SQL Server
            modelBuilder.Entity<Branch>()
                 .HasOne(b => b.Governorate)
                 .WithMany(g => g.Branches)
                 .HasForeignKey(b => b.GovernorateId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                 .HasOne(b => b.District)
                 .WithMany(d => d.Branches)
                 .HasForeignKey(b => b.DistrictId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                 .HasOne(b => b.Operator)
                 .WithMany(o => o.Branches)
                 .HasForeignKey(b => b.OperatorId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<District>()
                 .HasOne(d => d.Governorate)
                 .WithMany(g => g.Districts)
                 .HasForeignKey(d => d.GovernorateId)
                 .OnDelete(DeleteBehavior.Restrict);

            // Seeding Governorates
            modelBuilder.Entity<Governorate>().HasData(
                new Governorate { Id = 1, Name = "القاهرة", CenterLatitude = 30.0626, CenterLongitude = 31.2497 },
                new Governorate { Id = 2, Name = "الجيزة", CenterLatitude = 30.0131, CenterLongitude = 31.2089 },
                new Governorate { Id = 3, Name = "الإسكندرية", CenterLatitude = 31.2001, CenterLongitude = 29.9187 },
                new Governorate { Id = 4, Name = "الشرقية", CenterLatitude = 30.7226, CenterLongitude = 31.7231 },
                new Governorate { Id = 5, Name = "الدقهلية", CenterLatitude = 31.0409, CenterLongitude = 31.3819 },
                new Governorate { Id = 6, Name = "البحيرة", CenterLatitude = 30.8480, CenterLongitude = 30.3436 },
                new Governorate { Id = 7, Name = "المنوفية", CenterLatitude = 30.5965, CenterLongitude = 30.9876 },
                new Governorate { Id = 8, Name = "القليوبية", CenterLatitude = 30.3292, CenterLongitude = 31.2168 },
                new Governorate { Id = 9, Name = "الغربية", CenterLatitude = 30.8753, CenterLongitude = 31.0364 },
                new Governorate { Id = 10, Name = "كفر الشيخ", CenterLatitude = 31.1107, CenterLongitude = 30.9388 },
                new Governorate { Id = 11, Name = "الفيوم", CenterLatitude = 29.3084, CenterLongitude = 30.8428 },
                new Governorate { Id = 12, Name = "بني سويف", CenterLatitude = 29.0661, CenterLongitude = 31.0994 },
                new Governorate { Id = 13, Name = "المنيا", CenterLatitude = 28.0871, CenterLongitude = 30.7618 },
                new Governorate { Id = 14, Name = "أسيوط", CenterLatitude = 27.1809, CenterLongitude = 31.1837 },
                new Governorate { Id = 15, Name = "سوهاج", CenterLatitude = 26.5569, CenterLongitude = 31.6948 },
                new Governorate { Id = 16, Name = "قنا", CenterLatitude = 26.1551, CenterLongitude = 32.7160 },
                new Governorate { Id = 17, Name = "الأقصر", CenterLatitude = 25.6872, CenterLongitude = 32.6396 },
                new Governorate { Id = 18, Name = "أسوان", CenterLatitude = 24.0889, CenterLongitude = 32.8998 },
                new Governorate { Id = 19, Name = "البحر الأحمر", CenterLatitude = 24.6826, CenterLongitude = 34.1531 },
                new Governorate { Id = 20, Name = "الوادي الجديد", CenterLatitude = 25.4889, CenterLongitude = 29.0000 },
                new Governorate { Id = 21, Name = "مطروح", CenterLatitude = 31.3543, CenterLongitude = 27.2373 },
                new Governorate { Id = 22, Name = "شمال سيناء", CenterLatitude = 30.2841, CenterLongitude = 33.6259 },
                new Governorate { Id = 23, Name = "جنوب سيناء", CenterLatitude = 28.5388, CenterLongitude = 33.9981 },
                new Governorate { Id = 24, Name = "بورسعيد", CenterLatitude = 31.2565, CenterLongitude = 32.2841 },
                new Governorate { Id = 25, Name = "الإسماعيلية", CenterLatitude = 30.5965, CenterLongitude = 32.2715 },
                new Governorate { Id = 26, Name = "السويس", CenterLatitude = 29.9668, CenterLongitude = 32.5498 },
                new Governorate { Id = 27, Name = "دمياط", CenterLatitude = 31.4165, CenterLongitude = 31.8133 }
            );

            // Seeding ServiceTypes
            modelBuilder.Entity<ServiceType>().HasData(
                new ServiceType 
                { 
                    Id = 1, 
                    Name = "إصدار بطاقة الرقم القومي", 
                    Description = "الحصول على بطاقة الرقم القومي لأول مرة أو تجديدها", 
                    DurationMinutes = 30, 
                    RequiredDocuments = "استمارة الحصول على بطاقة الرقم القومي\nمستند إثبات الشخصية الحالي\nمستند إثبات محل الإقامة" 
                },
                new ServiceType 
                { 
                    Id = 2, 
                    Name = "إصدار شهادة الميلاد", 
                    Description = "الحصول على شهادة ميلاد مميكنة", 
                    DurationMinutes = 20, 
                    RequiredDocuments = "صورة بطاقة الرقم القومي للأب أو الأم\nطلب إصدار شهادة الميلاد" 
                },
                new ServiceType 
                { 
                    Id = 3, 
                    Name = "وثيقة الزواج", 
                    Description = "توثيق الزواج وإصدار الوثيقة", 
                    DurationMinutes = 45, 
                    RequiredDocuments = "صورة بطاقة الرقم القومي للزوج والزوجة\nالشهادة الصحية" 
                },
                new ServiceType 
                { 
                    Id = 4, 
                    Name = "وثيقة الطلاق", 
                    Description = "توثيق الطلاق وإصدار الوثيقة", 
                    DurationMinutes = 20, 
                    RequiredDocuments = "صورة بطاقة الرقم القومي للطرفين\nحكم المحكمة أو إقرار الطلاق" 
                },
                new ServiceType 
                { 
                    Id = 5, 
                    Name = "قيد عائلي", 
                    Description = "إصدار قيد عائلي مميكن", 
                    DurationMinutes = 15, 
                    RequiredDocuments = "صور بطاقات الرقم القومي لأفراد الأسرة\nشهادات الميلاد المميكنة للأولاد" 
                }
            );

            // Courts configurations
            modelBuilder.Entity<Court>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CourtDepartment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CourtService>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<LegalCase>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CaseTimelineEvent>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<QueueTicket>().HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<Court>().Property(e => e.RowVersion).IsRowVersion();
            modelBuilder.Entity<CourtDepartment>().Property(e => e.RowVersion).IsRowVersion();
            modelBuilder.Entity<CourtService>().Property(e => e.RowVersion).IsRowVersion();
            modelBuilder.Entity<Booking>().Property(e => e.RowVersion).IsRowVersion();
            modelBuilder.Entity<LegalCase>().Property(e => e.RowVersion).IsRowVersion();
            modelBuilder.Entity<CaseTimelineEvent>().Property(e => e.RowVersion).IsRowVersion();
            modelBuilder.Entity<QueueTicket>().Property(e => e.RowVersion).IsRowVersion();

            // Prevent multiple cascade paths in SQL Server
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Court)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CourtId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Department)
                .WithMany(d => d.Bookings)
                .HasForeignKey(b => b.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LegalCase>()
                .HasOne(lc => lc.Court)
                .WithMany(c => c.LegalCases)
                .HasForeignKey(lc => lc.CourtId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LegalCase>()
                .HasOne(lc => lc.Department)
                .WithMany(d => d.LegalCases)
                .HasForeignKey(lc => lc.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QueueTicket>()
                .HasOne(qt => qt.Court)
                .WithMany(c => c.QueueTickets)
                .HasForeignKey(qt => qt.CourtId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QueueTicket>()
                .HasOne(qt => qt.Booking)
                .WithOne(b => b.QueueTicket)
                .HasForeignKey<QueueTicket>(qt => qt.BookingId)
                .OnDelete(DeleteBehavior.NoAction);
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
}
