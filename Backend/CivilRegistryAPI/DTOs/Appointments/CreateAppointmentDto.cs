using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.DTOs.Appointments
{
    public class CreateAppointmentDto
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan TimeSlot { get; set; }
    }
}
