using System.ComponentModel.DataAnnotations;

namespace Maw3dyCare.ViewModels
{
    public class ReportVM
    {
        [Required(ErrorMessage = "Your name is required")]
        [MaxLength(100)]
        public string PatientName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^01[0125][0-9]{8}$",
            ErrorMessage = "Must start with 010/011/012/015")]
        public string PatientPhone { get; set; }

        [Required(ErrorMessage = "Hospital name is required")]
        [MaxLength(100)]
        public string HospitalName { get; set; }

        [Required(ErrorMessage = "Please write your complaint")]
        public string Complaint { get; set; }
    }

    public class ReportListVM
    {
        public int ReportId { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string HospitalName { get; set; }
        public string Complaint { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
