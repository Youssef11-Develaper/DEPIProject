using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Maw3dyCare.Models.Maw3dyCareDB
{
    
    public class ApplicationAdmin : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
