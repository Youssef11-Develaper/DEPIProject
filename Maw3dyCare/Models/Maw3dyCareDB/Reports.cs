using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maw3dyCare.Models.Maw3dyCareDB
{
    // جدول الابلاغات/الشكاوي من المرضى
    public class Reports
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PatientName { get; set; }

        [Required]
        [RegularExpression(@"^01[0125][0-9]{8}$",
            ErrorMessage = "Invalid Egyptian phone number")]
        public string PatientPhone { get; set; }

        [Required]
        [MaxLength(100)]
        public string HospitalName { get; set; }

        [Required]
        public string Complaint { get; set; }

        // هل الأدمن شافه ولا لأ
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
