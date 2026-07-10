using System.ComponentModel.DataAnnotations;

namespace Mawidy.Application.Hospitals.ViewModels
{
    public class HospitalLoginVM
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
