namespace Mawidy.Application.DTOs.Courts;

public class CourtDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public string Status { get; set; } = string.Empty;
    public int QueueCount { get; set; }
    public string WaitTime { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int SessionsToday { get; set; }
    public string Phone { get; set; } = string.Empty;
}
