using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        [Range(1, 5)]
        public int Stars { get; set; }

        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
