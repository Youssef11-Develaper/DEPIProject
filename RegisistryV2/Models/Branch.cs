using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegisistryV2.Models
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [ForeignKey("Governorate")]
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }
        public ICollection<BranchSchedule> Schedules { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<BranchCapacity> Capacities { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
        public ICollection<BranchHoliday> Holidays { get; set; }
        public ICollection<ServiceUnavailability> ServiceUnavailabilities { get; set; }
    }
}
