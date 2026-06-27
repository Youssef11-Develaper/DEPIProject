using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.DTOs.Auth
{
    public class UpdateProfileDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public int GovernorateId { get; set; }

        [Required]
        public string Area { get; set; } = string.Empty;
    }
}
