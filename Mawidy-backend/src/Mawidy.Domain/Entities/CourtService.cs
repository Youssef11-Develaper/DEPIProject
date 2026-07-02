namespace Mawidy.Domain.Entities;

public class CourtService : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequiredDocumentsJson { get; set; } = "[]";
    public TimeSpan EstimatedTime { get; set; }

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
