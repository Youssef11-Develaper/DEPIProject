using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mawidy.Domain.Entities.Hospitals
{
    // ???? ??????? ???????? ?? ?????
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
