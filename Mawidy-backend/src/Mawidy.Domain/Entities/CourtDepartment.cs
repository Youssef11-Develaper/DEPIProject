namespace Mawidy.Domain.Entities;

public class CourtDepartment : BaseEntity
{
    public Guid CourtId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public Court Court { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<LegalCase> LegalCases { get; set; } = new List<LegalCase>();
}
