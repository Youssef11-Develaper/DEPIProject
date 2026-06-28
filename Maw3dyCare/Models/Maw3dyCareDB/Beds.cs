using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maw3dyCare.Models.Maw3dyCareDB
{
    public class Beds
    {
        [Key]   
        public int BedId { set; get; }

        [Required]
        public String BedNumber { set; get; }
        [Required]

        public String Status { set; get; } = "Available";
        [ForeignKey("Hospitals")]
        public int HospitalId { set; get; }
        public Hospitals Hospitals { set; get; }
        [ForeignKey("BedTypes")]
        public int BedTypeId { set; get; }

        public BedTypes? BedTypes { set; get; }
    }
}
