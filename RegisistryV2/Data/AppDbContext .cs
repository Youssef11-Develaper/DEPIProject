using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Models;

namespace RegisistryV2.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BranchSchedule> BranchSchedules { get; set; }
        public DbSet<BranchCapacity> BranchCapacities { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<BranchHoliday> BranchHolidays { get; set; }
        public DbSet<ServiceUnavailability> ServiceUnavailabilities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Governorates
            modelBuilder.Entity<Governorate>().HasData(
                new Governorate { Id = 1, Name = "القاهرة", CenterLatitude = 30.0626, CenterLongitude = 31.2497 },
                new Governorate { Id = 2, Name = "الجيزة", CenterLatitude = 30.0131, CenterLongitude = 31.2089 },
                new Governorate { Id = 3, Name = "الإسكندرية", CenterLatitude = 31.2001, CenterLongitude = 29.9187 },
                new Governorate { Id = 4, Name = "الشرقية", CenterLatitude = 30.7226, CenterLongitude = 31.7231 },
                new Governorate { Id = 5, Name = "الدقهلية", CenterLatitude = 31.0409, CenterLongitude = 31.3819 }
            );

            // Branches
            modelBuilder.Entity<Branch>().HasData(
                new Branch { Id = 1, Name = "سجل مدني مصر الجديدة", Address = "شارع الأهرام، مصر الجديدة", Latitude = 30.0890, Longitude = 31.3233, GovernorateId = 1 },
                new Branch { Id = 2, Name = "سجل مدني مدينة نصر", Address = "شارع عباس العقاد، مدينة نصر", Latitude = 30.0626, Longitude = 31.3411, GovernorateId = 1 },
                new Branch { Id = 3, Name = "سجل مدني الدقي", Address = "شارع التحرير، الدقي", Latitude = 30.0408, Longitude = 31.2111, GovernorateId = 2 },
                new Branch { Id = 4, Name = "سجل مدني الإسكندرية", Address = "شارع النصر، الإسكندرية", Latitude = 31.2001, Longitude = 29.9187, GovernorateId = 3 }
            );

            // BranchSchedules
            modelBuilder.Entity<BranchSchedule>().HasData(
                // فرع 1 - الأحد للخميس
                new BranchSchedule { Id = 1, BranchId = 1, DayOfWeek = DayOfWeek.Sunday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(10, 0, 0), PeakEndTime = new TimeSpan(12, 0, 0), MaxAppointmentsPerSlot = 3 },
                new BranchSchedule { Id = 2, BranchId = 1, DayOfWeek = DayOfWeek.Monday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(10, 0, 0), PeakEndTime = new TimeSpan(12, 0, 0), MaxAppointmentsPerSlot = 3 },
                new BranchSchedule { Id = 3, BranchId = 1, DayOfWeek = DayOfWeek.Tuesday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(10, 0, 0), PeakEndTime = new TimeSpan(12, 0, 0), MaxAppointmentsPerSlot = 3 },
                new BranchSchedule { Id = 4, BranchId = 1, DayOfWeek = DayOfWeek.Wednesday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(10, 0, 0), PeakEndTime = new TimeSpan(12, 0, 0), MaxAppointmentsPerSlot = 3 },
                new BranchSchedule { Id = 5, BranchId = 1, DayOfWeek = DayOfWeek.Thursday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(10, 0, 0), PeakEndTime = new TimeSpan(12, 0, 0), MaxAppointmentsPerSlot = 3 },

                // فرع 2
                new BranchSchedule { Id = 6, BranchId = 2, DayOfWeek = DayOfWeek.Sunday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(9, 0, 0), PeakEndTime = new TimeSpan(11, 0, 0), MaxAppointmentsPerSlot = 2 },
                new BranchSchedule { Id = 7, BranchId = 2, DayOfWeek = DayOfWeek.Monday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(9, 0, 0), PeakEndTime = new TimeSpan(11, 0, 0), MaxAppointmentsPerSlot = 2 },
                new BranchSchedule { Id = 8, BranchId = 2, DayOfWeek = DayOfWeek.Tuesday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(9, 0, 0), PeakEndTime = new TimeSpan(11, 0, 0), MaxAppointmentsPerSlot = 2 },
                new BranchSchedule { Id = 9, BranchId = 2, DayOfWeek = DayOfWeek.Wednesday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(9, 0, 0), PeakEndTime = new TimeSpan(11, 0, 0), MaxAppointmentsPerSlot = 2 },
                new BranchSchedule { Id = 10, BranchId = 2, DayOfWeek = DayOfWeek.Thursday, OpenTime = new TimeSpan(8, 0, 0), CloseTime = new TimeSpan(15, 0, 0), PeakStartTime = new TimeSpan(9, 0, 0), PeakEndTime = new TimeSpan(11, 0, 0), MaxAppointmentsPerSlot = 2 }
            );

            // ServiceTypes
            modelBuilder.Entity<ServiceType>().HasData(
               new ServiceType
               {
                   Id = 1,
                   Name = "بطاقة الرقم القومي",
                   Description = "استخراج أو تجديد بطاقة الرقم القومي",
                   DurationMinutes = 30,
                   RequiredDocuments = "بطاقة الرقم القومي منتهية الصلاحية\nصورة شخصية حديثة\nشهادة الميلاد"
               },

new ServiceType
{
    Id = 2,
    Name = "شهادة الميلاد",
    Description = "استخراج شهادة ميلاد",
    DurationMinutes = 20,
    RequiredDocuments = "بطاقة الرقم القومي للأب أو الأم\nعقد الزواج\nشهادة الميلاد الأصلية من المستشفى"
},

new ServiceType
{
    Id = 3,
    Name = "عقد الزواج",
    Description = "توثيق عقد الزواج",
    DurationMinutes = 45,
    RequiredDocuments = "بطاقة الرقم القومي للطرفين\nشهادة ميلاد الطرفين\nموافقة ولي الأمر لو العروسة أقل من 21 سنة"
},

new ServiceType
{
    Id = 4,
    Name = "شهادة الوفاة",
    Description = "استخراج شهادة وفاة",
    DurationMinutes = 20,
    RequiredDocuments = "بطاقة الرقم القومي للمتوفي\nتقرير الوفاة من المستشفى أو الطبيب\nبطاقة الرقم القومي لمقدم الطلب"
},

new ServiceType
{
    Id = 5,
    Name = "قيد الأسرة",
    Description = "استخراج قيد الأسرة",
    DurationMinutes = 15,
    RequiredDocuments = "بطاقة الرقم القومي\nعقد الزواج"
});
        }
    }
}
