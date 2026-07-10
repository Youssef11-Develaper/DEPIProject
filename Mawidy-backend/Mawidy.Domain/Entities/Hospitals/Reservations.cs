using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mawidy.Domain.Entities.Hospitals
{
    public class Reservations
    {
        [Key]
        public int ReservationId { set; get; }
        [Required]
        public string PatientName { set; get; }
        [Required]
        [RegularExpression(@"^01[0125][0-9]{8}$")]
        public String PatientPhone { set; get; }
        [Required]
        public String CaseDescription { set; get; }
        [Required]

        public String Status { set; get; }
        [Range(1, 120, ErrorMessage = "Arrival Time between 1 and 120 minutes")]
        public int ETA { set; get; }
        public DateTime ReservedAt { set; get; } = DateTime.Now;
        public DateTime ExpiresAt { set; get; } = DateTime.Now.AddMinutes(30);
        [ForeignKey("Hospitals")]
        public int HospitalId { get; set; }
        public Hospitals? Hospitals { get; set; }
        [ForeignKey("BedTypes")]
        public int BedTypeId { get; set; }
        public BedTypes? BedTypes { get; set; }
        [ForeignKey("Beds")]
        public int? BedId { get; set; }
        public Beds? Bed { get; set; }

        
    }
}
