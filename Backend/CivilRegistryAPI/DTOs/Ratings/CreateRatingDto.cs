using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.DTOs.Ratings
{
    public class CreateRatingDto
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }

        public string? Comment { get; set; }
    }
}
