using Mawidy.Domain.Enums;

namespace Mawidy.Domain.Entities;

public class CaseTimelineEvent : BaseEntity
{
    public Guid LegalCaseId { get; set; }
    
    public TimelineEventStatus Status { get; set; }
    public string Title { get; set; } = string.Empty; // e.g., "إيداع الدعوى"
    public DateTime EventDate { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public LegalCase LegalCase { get; set; } = null!;
}
