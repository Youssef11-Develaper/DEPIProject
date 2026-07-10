
namespace Mawidy.Application.Hospitals.ViewModels
{
    public class SettingsVM
    {
        public string AdminId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public string Initials => FullName?.Length >= 2
            ? FullName.Substring(0, 2).ToUpper()
            : FullName?.ToUpper() ?? "AD";
    }
}
