using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ComplaintStatus Status { get; set; }
        public string? AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
    }

    public enum ComplaintStatus
    {
        Submitted,
        UnderReview,
        Resolved,
        Closed
    }
}
