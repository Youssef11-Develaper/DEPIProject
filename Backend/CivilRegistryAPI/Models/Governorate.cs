using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.Models
{
    public class Governorate
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }

        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
