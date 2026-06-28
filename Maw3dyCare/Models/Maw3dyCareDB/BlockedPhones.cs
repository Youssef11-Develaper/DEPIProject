using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maw3dyCare.Models.Maw3dyCareDB
{
    // جدول الأرقام المحظورة من الحجز
    public class BlockedPhones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^01[0125][0-9]{8}$")]
        public string Phone { get; set; }

        [ForeignKey("Hospitals")]
        public int HospitalId { get; set; }
        public Hospitals? Hospital { get; set; }

        public string? Reason { get; set; }
        public DateTime BlockedAt { get; set; } = DateTime.Now;
    }
}
