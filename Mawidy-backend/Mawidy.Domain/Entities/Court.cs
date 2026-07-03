using Mawidy.Domain.Enums;

namespace Mawidy.Domain.Entities;

public class Court : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public CourtType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Phone { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<CourtDepartment> Departments { get; set; } = new List<CourtDepartment>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<LegalCase> LegalCases { get; set; } = new List<LegalCase>();
    public ICollection<QueueTicket> QueueTickets { get; set; } = new List<QueueTicket>();
}
