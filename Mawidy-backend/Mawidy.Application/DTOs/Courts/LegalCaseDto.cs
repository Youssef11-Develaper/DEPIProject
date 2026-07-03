namespace Mawidy.Application.DTOs.Courts;

public class LegalCaseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<CaseTimelineEventDto> Timeline { get; set; } = new();
}

public class CaseTimelineEventDto
{
    public string Status { get; set; } = string.Empty; // "done", "active", "pending"
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? Note { get; set; }
}
