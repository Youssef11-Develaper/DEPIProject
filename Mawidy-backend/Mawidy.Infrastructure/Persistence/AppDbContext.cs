using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Infrastructure.Persistence
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
                new Governorate { Id = 1, Name = "???????", CenterLatitude = 30.0626, CenterLongitude = 31.2497 },
                new Governorate { Id = 2, Name = "??????", CenterLatitude = 30.0131, CenterLongitude = 31.2089 },
                new Governorate { Id = 3, Name = "??????????", CenterLatitude = 31.2001, CenterLongitude = 29.9187 },
                new Governorate { Id = 4, Name = "???????", CenterLatitude = 30.7226, CenterLongitude = 31.7231 },
                new Governorate { Id = 5, Name = "????????", CenterLatitude = 31.0409, CenterLongitude = 31.3819 },
                new Governorate { Id = 6, Name = "???????", CenterLatitude = 30.8480, CenterLongitude = 30.3436 },
                new Governorate { Id = 7, Name = "????????", CenterLatitude = 30.5965, CenterLongitude = 30.9876 },
                new Governorate { Id = 8, Name = "?????????", CenterLatitude = 30.3292, CenterLongitude = 31.2168 },
                new Governorate { Id = 9, Name = "???????", CenterLatitude = 30.8753, CenterLongitude = 31.0364 },
                new Governorate { Id = 10, Name = "??? ?????", CenterLatitude = 31.1107, CenterLongitude = 30.9388 },
                new Governorate { Id = 11, Name = "??????", CenterLatitude = 29.3084, CenterLongitude = 30.8428 },
                new Governorate { Id = 12, Name = "??? ????", CenterLatitude = 29.0661, CenterLongitude = 31.0994 },
                new Governorate { Id = 13, Name = "??????", CenterLatitude = 28.0871, CenterLongitude = 30.7618 },
                new Governorate { Id = 14, Name = "?????", CenterLatitude = 27.1809, CenterLongitude = 31.1837 },
                new Governorate { Id = 15, Name = "?????", CenterLatitude = 26.5569, CenterLongitude = 31.6948 },
                new Governorate { Id = 16, Name = "???", CenterLatitude = 26.1551, CenterLongitude = 32.7160 },
                new Governorate { Id = 17, Name = "??????", CenterLatitude = 25.6872, CenterLongitude = 32.6396 },
                new Governorate { Id = 18, Name = "?????", CenterLatitude = 24.0889, CenterLongitude = 32.8998 },
                new Governorate { Id = 19, Name = "????? ??????", CenterLatitude = 24.6826, CenterLongitude = 34.1531 },
                new Governorate { Id = 20, Name = "?????? ??????", CenterLatitude = 25.4889, CenterLongitude = 29.0000 },
                new Governorate { Id = 21, Name = "?????", CenterLatitude = 31.3543, CenterLongitude = 27.2373 },
                new Governorate { Id = 22, Name = "???? ?????", CenterLatitude = 30.2841, CenterLongitude = 33.6259 },
                new Governorate { Id = 23, Name = "???? ?????", CenterLatitude = 28.5388, CenterLongitude = 33.9981 },
                new Governorate { Id = 24, Name = "???????", CenterLatitude = 31.2565, CenterLongitude = 32.2841 },
                new Governorate { Id = 25, Name = "???????????", CenterLatitude = 30.5965, CenterLongitude = 32.2715 },
                new Governorate { Id = 26, Name = "??????", CenterLatitude = 29.9668, CenterLongitude = 32.5498 },
                new Governorate { Id = 27, Name = "?????", CenterLatitude = 31.4165, CenterLongitude = 31.8133 }
            );

            modelBuilder.Entity<ServiceType>().HasData(
                new ServiceType { Id = 1, Name = "????? ????? ??????", Description = "??????? ?? ????? ????? ????? ??????", DurationMinutes = 30, RequiredDocuments = "????? ????? ?????? ?????? ????????\n???? ????? ?????\n????? ???????" },
                new ServiceType { Id = 2, Name = "????? ???????", Description = "??????? ????? ?????", DurationMinutes = 20, RequiredDocuments = "????? ????? ?????? ???? ?? ????\n??? ??????\n????? ??????? ??????? ?? ????????" },
                new ServiceType { Id = 3, Name = "??? ??????", Description = "????? ??? ??????", DurationMinutes = 45, RequiredDocuments = "????? ????? ?????? ???????\n????? ????? ???????\n?????? ??? ????? ?? ??????? ??? ?? 21 ???" },
                new ServiceType { Id = 4, Name = "????? ??????", Description = "??????? ????? ????", DurationMinutes = 20, RequiredDocuments = "????? ????? ?????? ???????\n????? ?????? ?? ???????? ?? ??????\n????? ????? ?????? ????? ?????" },
                new ServiceType { Id = 5, Name = "??? ??????", Description = "??????? ??? ??????", DurationMinutes = 15, RequiredDocuments = "????? ????? ??????\n??? ??????" }
            );
        }
    }
}

