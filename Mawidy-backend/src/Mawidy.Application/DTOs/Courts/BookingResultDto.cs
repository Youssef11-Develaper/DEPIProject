namespace Mawidy.Application.DTOs.Courts;

public class BookingResultDto
{
    public Guid BookingId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public int QueueNumber { get; set; }
    public string DateString { get; set; } = string.Empty;
    public string TimeString { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}
