namespace TelecomBranches.Models;

public enum AppointmentStatus { Pending, Confirmed, Cancelled, Completed }

public class Appointment
{
    public int               Id              { get; set; }
    public int               BranchId        { get; set; }
    public string            ServiceKey      { get; set; } = string.Empty;
    public string            CustomerName    { get; set; } = string.Empty;
    public string            CustomerPhone   { get; set; } = string.Empty;
    public DateTime          AppointmentDate { get; set; }
    public TimeSpan          AppointmentTime { get; set; }
    public AppointmentStatus Status          { get; set; } = AppointmentStatus.Pending;
    public DateTime          CreatedAt       { get; set; } = DateTime.UtcNow;
    public string?           Notes           { get; set; }

    public Branch Branch { get; set; } = null!;
}

public class VirtualQueueEntry
{
    public int      Id            { get; set; }
    public int      BranchId      { get; set; }
    public string   CustomerName  { get; set; } = string.Empty;
    public string   CustomerPhone { get; set; } = string.Empty;
    public string   ServiceKey    { get; set; } = string.Empty;
    public int      Position      { get; set; }
    public bool     IsActive      { get; set; } = true;
    public DateTime JoinedAt      { get; set; } = DateTime.UtcNow;

    public Branch Branch { get; set; } = null!;
}
