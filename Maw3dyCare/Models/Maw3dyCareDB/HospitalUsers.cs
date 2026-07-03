using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maw3dyCare.Models.Maw3dyCareDB
{
    public class HospitalUsers
    {
        [Key]
        public int UserId { set; get; }
        [Required]
        [MaxLength(100)]
        public String FullName { set; get; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
    )]

        public String Email { set; get; }
        [Required]

        public string PasswordHash { set; get; }
    public bool IsActive { set; get; }
        public DateTime CreatedAt { set; get; } = DateTime.Now;
        [ForeignKey("Hospitals")]
        public int HospitalId { get; set; }
        public Hospitals? Hospitals { get; set; }
    }
}
