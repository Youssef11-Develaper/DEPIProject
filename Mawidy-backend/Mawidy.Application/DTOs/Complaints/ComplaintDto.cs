

namespace Mawidy.Application.DTOs.Complaints
{
    public class ComplaintDto
    {
        public int Id { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminResponse { get; set; }
        public int? AppointmentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

