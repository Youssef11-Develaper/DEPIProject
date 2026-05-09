using Mawidy.Domain.Enums;

namespace Mawidy.Domain.Entities;

public class QueueTicket : BaseEntity
{
    public Guid CourtId { get; set; }
    public Guid? BookingId { get; set; }
    
    public int TicketNumber { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public TimeSpan EstimatedWaitTime { get; set; }
    
    public QueueTicketStatus Status { get; set; } = QueueTicketStatus.Waiting;

    // Navigation properties
    public Court Court { get; set; } = null!;
    public Booking? Booking { get; set; }
}
