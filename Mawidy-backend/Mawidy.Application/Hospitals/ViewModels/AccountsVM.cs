namespace Mawidy.Application.Hospitals.ViewModels
{
    public class AccountCardVM
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string HospitalName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Initials => FullName?.Length >= 2
            ? FullName.Substring(0, 2).ToUpper()
            : FullName?.ToUpper() ?? "??";
    }

    public class ResetPasswordVM
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password is required")]
        [System.ComponentModel.DataAnnotations.MinLength(8, ErrorMessage = "Min 8 characters")]
        public string NewPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Please confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword",
            ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
