using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.Models
{
    public class ServiceType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public string? RequiredDocuments { get; set; } 
        public ICollection<Appointment> Appointments { get; set; }
    }
}
