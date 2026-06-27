using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.ViewModel.Admin
{
    public class ServiceTypeViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(5, 120)]
        public int DurationMinutes { get; set; }

        public string? RequiredDocuments { get; set; } 
    }
}
