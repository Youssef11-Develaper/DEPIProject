namespace Mawidy.Application.DTOs.Courts;

public class LiveQueueDto
{
    public int CurrentQueueNumber { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public List<QueueTicketDto> WaitingList { get; set; } = new();
}

public class QueueTicketDto
{
    public int TicketNumber { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string WaitTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "current", "wait"
}
