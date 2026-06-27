using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.DTOs.Complaints
{
    public class CreateComplaintDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public int? AppointmentId { get; set; }
    }
}
