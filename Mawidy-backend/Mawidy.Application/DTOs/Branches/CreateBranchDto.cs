using System.ComponentModel.DataAnnotations;

namespace Mawidy.Application.DTOs.Branches
{
    public class CreateBranchDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required]
        public int GovernorateId { get; set; }
    }
}

