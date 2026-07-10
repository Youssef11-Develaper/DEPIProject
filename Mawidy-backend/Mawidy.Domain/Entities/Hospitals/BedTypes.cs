using System.ComponentModel.DataAnnotations;

namespace Mawidy.Domain.Entities.Hospitals
{
    public class BedTypes
    {
        [Key]
        public int BedTypeId { set; get; }
        [Required]
        [MaxLength(100)]

        public String Name { set; get; }
        public String? Description { set; get; }
        public ICollection<Beds>? Beds { get; set; }       
        public ICollection<Reservations>? Reservations { get; set; }
    }
}
