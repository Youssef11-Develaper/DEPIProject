using Mawidy.Domain.Enums;

namespace Mawidy.Domain.Entities
{
    public class Complaint
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComplaintStatus Status { get; set; }
        public string? AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
