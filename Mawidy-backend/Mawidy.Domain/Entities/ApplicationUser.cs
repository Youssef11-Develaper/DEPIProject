using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Mawidy.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [StringLength(14, MinimumLength = 14)]
        public string NationalId { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public int? GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }

        public string? Area { get; set; }

        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

        // Shared Attributes
        public string? City { get; set; }
        public string? Address { get; set; }

        // Bank Specific Attributes
        public bool IsBankEmployee { get; set; } = false;
        public int? BankBranchId { get; set; }
        public Branch? BankBranch { get; set; }
        public ICollection<Appointment> BankAppointments { get; set; } = new List<Appointment>();

        // Hospital Specific Attributes
        public int? HospitalId { get; set; }
        public Hospitals.Hospitals? Hospital { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
