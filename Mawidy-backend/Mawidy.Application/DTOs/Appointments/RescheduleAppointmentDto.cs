using System.ComponentModel.DataAnnotations;

namespace Mawidy.Application.DTOs.Appointments
{
    public class RescheduleAppointmentDto
    {
        [Required]
        public DateTime NewDate { get; set; }

        [Required]
        public TimeSpan NewTimeSlot { get; set; }
    }
}

