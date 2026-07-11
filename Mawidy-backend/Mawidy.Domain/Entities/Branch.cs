using System.ComponentModel.DataAnnotations;
using Mawidy.Domain.Enums;

namespace Mawidy.Domain.Entities
{
    public class Branch
    {
        public int Id { get; set; }

        public SystemType SystemType { get; set; } = SystemType.CivilRegistry;

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

        // Telecom properties
        public int OperatorId { get; set; }
        public int DistrictId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public BranchStatus Status { get; set; }
        public int QueueCount { get; set; }
        public string WaitTime { get; set; } = string.Empty;
        public double Rating { get; set; }

        // Added Bank properties for unification
        public string NameEn { get; set; } = string.Empty;
        public string CityEn { get; set; } = string.Empty;
        public string CityAr { get; set; } = string.Empty;
        public string AddressEn { get; set; } = string.Empty;
        public string AddressAr { get; set; } = string.Empty;
        public string HoursEn { get; set; } = string.Empty;
        public string HoursAr { get; set; } = string.Empty;

        public Operator Operator { get; set; } = null!;
        public District District { get; set; } = null!;

        public ICollection<VirtualQueueEntry> QueueEntries { get; set; } = new List<VirtualQueueEntry>();
    }
}
