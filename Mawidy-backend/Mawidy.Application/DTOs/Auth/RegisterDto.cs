using System.ComponentModel.DataAnnotations;

namespace Mawidy.Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(14, MinimumLength = 14)]
        public string NationalId { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public int GovernorateId { get; set; }

        [Required]
        public string Area { get; set; } = string.Empty;
    }
}

