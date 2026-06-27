using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.ViewModel
{
    public class ProfileViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public int GovernorateId { get; set; }

        [Required]
        public string Area { get; set; }

        public string Email { get; set; }
        public string NationalId { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int TotalComplaints { get; set; }
    }
}
