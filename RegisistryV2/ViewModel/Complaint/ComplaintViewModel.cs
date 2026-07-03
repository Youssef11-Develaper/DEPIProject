using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.ViewModel.Complaint
{
    public class ComplaintViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public int? AppointmentId { get; set; }

    }
}
