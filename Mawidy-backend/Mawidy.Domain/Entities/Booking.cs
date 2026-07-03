using Mawidy.Domain.Enums;

namespace Mawidy.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable, as booking might be done without an account in some cases
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? CaseNumber { get; set; } // Optional
    
    public Guid CourtId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid ServiceId { get; set; }
    
    public DateTime BookingDate { get; set; }
    public TimeSpan TimeSlot { get; set; }
    public string? Notes { get; set; }
    
    public bool WantsSms { get; set; }
    public bool WantsReminder { get; set; }
    
    public string QrCode { get; set; } = string.Empty;
    public int QueueNumber { get; set; }
    
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    // Navigation properties
    public Court Court { get; set; } = null!;
    public CourtDepartment Department { get; set; } = null!;
    public CourtService Service { get; set; } = null!;
    public QueueTicket? QueueTicket { get; set; }
}
