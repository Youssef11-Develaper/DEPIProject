using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.Models
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
    }
}
