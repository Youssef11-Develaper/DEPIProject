using Mawidy.Domain.Enums;

namespace Mawidy.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int ServiceTypeId { get; set; }
        public ServiceType ServiceType { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public AppointmentStatus Status { get; set; }
        public bool IsNotified { get; set; }
        public DateTime CreatedAt { get; set; }

        public Rating? Rating { get; set; }
    }
}
