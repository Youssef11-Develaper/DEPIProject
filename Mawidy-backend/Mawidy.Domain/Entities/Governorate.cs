using System.ComponentModel.DataAnnotations;

namespace Mawidy.Domain.Entities
{
    public class Governorate
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }

        // Telecom properties
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<District> Districts { get; set; } = new List<District>();
    }

    public class District
    {
        public int Id { get; set; }
        public int GovernorateId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public Governorate Governorate { get; set; } = null!;
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}
