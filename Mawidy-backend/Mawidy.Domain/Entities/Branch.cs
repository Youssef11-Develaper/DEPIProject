using System.ComponentModel.DataAnnotations;

namespace Mawidy.Domain.Entities
{
    public class Branch
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; } = null!;

        public ICollection<BranchSchedule> Schedules { get; set; } = new List<BranchSchedule>();
        public ICollection<BranchHoliday> Holidays { get; set; } = new List<BranchHoliday>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<ServiceUnavailability> ServiceUnavailabilities { get; set; } = new List<ServiceUnavailability>();
    }
}
