using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.ViewModel.Admin
{
    public class BranchViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [Required]
        public int GovernorateId { get; set; }
    }
}
