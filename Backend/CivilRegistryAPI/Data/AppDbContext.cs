using CivilRegistryAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CivilRegistryAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
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

            // Seeding
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

            modelBuilder.Entity<ServiceType>().HasData(
                new ServiceType { Id = 1, Name = "بطاقة الرقم القومي", Description = "استخراج أو تجديد بطاقة الرقم القومي", DurationMinutes = 30, RequiredDocuments = "بطاقة الرقم القومي منتهية الصلاحية\nصورة شخصية حديثة\nشهادة الميلاد" },
                new ServiceType { Id = 2, Name = "شهادة الميلاد", Description = "استخراج شهادة ميلاد", DurationMinutes = 20, RequiredDocuments = "بطاقة الرقم القومي للأب أو الأم\nعقد الزواج\nشهادة الميلاد الأصلية من المستشفى" },
                new ServiceType { Id = 3, Name = "عقد الزواج", Description = "توثيق عقد الزواج", DurationMinutes = 45, RequiredDocuments = "بطاقة الرقم القومي للطرفين\nشهادة ميلاد الطرفين\nموافقة ولي الأمر لو العروسة أقل من 21 سنة" },
                new ServiceType { Id = 4, Name = "شهادة الوفاة", Description = "استخراج شهادة وفاة", DurationMinutes = 20, RequiredDocuments = "بطاقة الرقم القومي للمتوفي\nتقرير الوفاة من المستشفى أو الطبيب\nبطاقة الرقم القومي لمقدم الطلب" },
                new ServiceType { Id = 5, Name = "قيد الأسرة", Description = "استخراج قيد الأسرة", DurationMinutes = 15, RequiredDocuments = "بطاقة الرقم القومي\nعقد الزواج" }
            );
        }
    }
}
