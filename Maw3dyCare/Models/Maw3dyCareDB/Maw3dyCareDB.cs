using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Maw3dyCare.Models.Maw3dyCareDB
{

    public class Maw3dyCareDB : IdentityDbContext<ApplicationAdmin>
    {
        public DbSet<Hospitals>Hospital { set; get; }
        public DbSet<Beds>Bed { set; get; }
        public DbSet<BedTypes>BedType { set; get; }
        public DbSet<Reservations>Reservation { set; get; }
        public DbSet<HospitalUsers> HospitalUser { set; get; }
        public DbSet<BlockedPhones> BlockedPhone { get; set; }
        public DbSet<Reports> Report { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        {
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Maw3dyCare;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //seed Data for BedTypes
            modelBuilder.Entity<BedTypes>().HasData(
                new BedTypes { BedTypeId = 1, Name = "ICU", Description = "General intensive care" },
                new BedTypes { BedTypeId = 2, Name = "CICU", Description = "Intensive care cardiac" },
                new BedTypes { BedTypeId = 3, Name = "NICU", Description = "Children’s nursery" },
                new BedTypes { BedTypeId = 4, Name = "VENT", Description = "Artificial respirator" });
            //seed Data for Hospitals
            modelBuilder.Entity<Hospitals>().HasData(
                new Hospitals { HospitalId = 1, Name = "Fayoum University Hospital", Address = "Haya El-Gamaa , Fayoum", City = "Fayoum", Latitude = 29.32141m, Longitude = 30.83397m, Phone = "01012345678", Email = "Fayoum_University@gmail.com", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Hospitals
                {
                    HospitalId = 2,
                    Name = "EL_Haya Hospital",
                    Address = "Dallah , Fayoum",
                    City = "Fayoum",
                    Latitude = 29.32343m,
                    Longitude = 30.85756m,
                    Phone = "01098765432",
                    Email = "El_Haya@gmail.com",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 2, 3)
                },
                new Hospitals { HospitalId = 3, Name = "Al Salam Specialized Hospital", Address = "EL-Shader , Fayoum", City = "Fayoum", Latitude = 29.31056m, Longitude = 30.83940m, Phone = "01111111111", Email = "Al_Salam_Specialized@gmail.com", IsActive = true, CreatedAt = new DateTime(2026, 4, 5) });
            //seed Data for  Beds
            modelBuilder.Entity<Beds>().HasData(
        //Fayoum University Hospital
        new Beds { BedId = 1, BedNumber = "ICU-01", Status = "Available", HospitalId = 1, BedTypeId = 1 },
        new Beds { BedId = 2, BedNumber = "ICU-02", Status = "Occupied", HospitalId = 1, BedTypeId = 1 },
        new Beds { BedId = 3, BedNumber = "CICU-01", Status = "Available", HospitalId = 1, BedTypeId = 2 },
        new Beds { BedId = 4, BedNumber = "NICU-01", Status = "Available", HospitalId = 1, BedTypeId = 3 },
        new Beds { BedId = 5, BedNumber = "VENT-01", Status = "Occupied", HospitalId = 1, BedTypeId = 4 },
        //EL_Haya Hospital
        new Beds { BedId = 6, BedNumber = "ICU-01", Status = "Available", HospitalId = 2, BedTypeId = 1 },
        new Beds { BedId = 7, BedNumber = "ICU-02", Status = "Available", HospitalId = 2, BedTypeId = 1 },
        new Beds { BedId = 8, BedNumber = "CICU-01", Status = "Occupied", HospitalId = 2, BedTypeId = 2 },
        new Beds { BedId = 9, BedNumber = "NICU-01", Status = "Available", HospitalId = 2, BedTypeId = 3 },
        //Al Salam Specialized Hospital
        new Beds { BedId = 10, BedNumber = "ICU-01", Status = "Available", HospitalId = 3, BedTypeId = 1 },
        new Beds { BedId = 11, BedNumber = "CICU-01", Status = "Occupied", HospitalId = 3, BedTypeId = 2 },
        new Beds { BedId = 12, BedNumber = "VENT-01", Status = "Available", HospitalId = 3, BedTypeId = 4 }
            );


           



        }



    }
}
