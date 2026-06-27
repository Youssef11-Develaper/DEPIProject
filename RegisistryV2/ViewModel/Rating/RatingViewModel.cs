using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.ViewModel.Rating
{
    public class RatingViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }

        public string Comment { get; set; }
    }
}
