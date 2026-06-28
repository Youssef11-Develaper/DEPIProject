using TelecomBranches.Domain.Enums;

namespace TelecomBranches.Domain.Entities;

public class Branch
{
    public int          Id            { get; set; }
    public int          OperatorId    { get; set; }
    public int          GovernorateId { get; set; }
    public int          DistrictId    { get; set; }
    public string       NameAr        { get; set; } = string.Empty;
    public string       Area          { get; set; } = string.Empty;
    public string       Address       { get; set; } = string.Empty;
    public double       DistanceKm    { get; set; }
    public BranchStatus Status        { get; set; }
    public int          QueueCount    { get; set; }
    public string       WaitTime      { get; set; } = string.Empty;
    public double       Rating        { get; set; }

    public Operator     Operator    { get; set; } = null!;
    public Governorate  Governorate { get; set; } = null!;
    public District     District    { get; set; } = null!;

    public ICollection<Appointment>       Appointments { get; set; } = new List<Appointment>();
    public ICollection<VirtualQueueEntry> QueueEntries { get; set; } = new List<VirtualQueueEntry>();
}
