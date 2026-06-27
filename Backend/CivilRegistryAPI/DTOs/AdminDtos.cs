using CivilRegistryAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.DTOs
{
    public class UpdateStatusDto
    {
        [Required]
        public AppointmentStatus Status { get; set; }
    }

    public class AddScheduleDto
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public TimeSpan PeakStartTime { get; set; }
        public TimeSpan PeakEndTime { get; set; }
        public int MaxAppointmentsPerSlot { get; set; }
    }

    public class AddHolidayDto
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class AddServiceUnavailabilityDto
    {
        [Required]
        public int ServiceTypeId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class CreateServiceDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(5, 120)]
        public int DurationMinutes { get; set; }
        public string? RequiredDocuments { get; set; }
    }
}
