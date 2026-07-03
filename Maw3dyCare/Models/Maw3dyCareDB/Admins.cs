using System.ComponentModel.DataAnnotations;

namespace Maw3dyCare.Models.Maw3dyCareDB
{
    public class Admins
    {
        [Key]
        public int AdminId { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

