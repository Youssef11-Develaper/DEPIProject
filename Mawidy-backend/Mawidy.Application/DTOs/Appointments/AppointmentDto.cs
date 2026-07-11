

namespace Mawidy.Application.DTOs.Appointments
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string StringId { get; set; } = string.Empty;
        public int SystemType { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserNationalId { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchAddress { get; set; } = string.Empty;
        public int ServiceTypeId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? RequiredDocuments { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public string Status { get; set; } = string.Empty;
        public int QueueNumber { get; set; }
        public bool IsNotified { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasRating { get; set; }
    }
}

