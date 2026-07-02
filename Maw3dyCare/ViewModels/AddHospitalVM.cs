using System.ComponentModel.DataAnnotations;

namespace Maw3dyCare.ViewModels
{
    public class AddHospitalVM
    {
        [Required(ErrorMessage = "Hospital name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(200)]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100)]
        public string City { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^01[0125][0-9]{8}$",
            ErrorMessage = "Phone must start with 010/011/012/015")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        public string? Description { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public bool IsActive { get; set; } = true;

      
        public int IcuAvailable { get; set; }
        public int IcuOccupied { get; set; }

        public int NicuAvailable { get; set; }
        public int NicuOccupied { get; set; }

        public int CicuAvailable { get; set; }
        public int CicuOccupied { get; set; }

        public int VentAvailable { get; set; }
        public int VentOccupied { get; set; }
    }

    public class CreateHospitalAccountVM
    {
        public int HospitalId { get; set; }
        public string HospitalName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
    }
}