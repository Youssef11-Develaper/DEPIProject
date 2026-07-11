using System.ComponentModel.DataAnnotations;

namespace Mawidy.Domain.Entities.Hospitals
{
    public class Hospitals
    {
        [Key]
        public int HospitalId { set; get; }
        [Required]
        [MaxLength(100)]

        public String Name { set; get; }
        [Required]
        [MaxLength(200)]

        public String Address { set; get; }
        [Required]
        [MaxLength(100)]

        public String City { set; get; }
        [Required]
        public decimal Latitude { set; get; }
        [Required]
        public decimal Longitude { set; get; }
        [Required]
        [RegularExpression(@"^01[0125][0-9]{8}$")]
        public String Phone { set; get; }
       
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
    )]
        
        public String Email { set; get; }
        public String? Description { set; get; }
        public bool IsActive { set; get; }
        public DateTime CreatedAt { set; get; } = DateTime.Now;
        public ICollection<Beds>? Beds { set; get; }
     
        public ICollection<Reservations>? Reservations { get; set; }
        public ICollection<Mawidy.Domain.Entities.ApplicationUser>? HospitalUsers { get; set; }
       
    }
}
