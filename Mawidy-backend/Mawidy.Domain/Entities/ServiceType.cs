using System.ComponentModel.DataAnnotations;

namespace Mawidy.Domain.Entities
{
    public class ServiceType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string? RequiredDocuments { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<ServiceUnavailability> ServiceUnavailabilities { get; set; } = new List<ServiceUnavailability>();
    }
}
