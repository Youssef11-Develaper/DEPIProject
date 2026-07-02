namespace Mawidy.Domain.Entities;

public class LegalCase : BaseEntity
{
    public string CaseNumber { get; set; } = string.Empty; // e.g., "2025/4821"
    public int Year { get; set; }
    
    public Guid CourtId { get; set; }
    public Guid DepartmentId { get; set; }
    
    public string Type { get; set; } = string.Empty; // e.g., "دعوى مدنية"
    public string Status { get; set; } = string.Empty;
    
    public string Plaintiff { get; set; } = string.Empty;
    public string Defendant { get; set; } = string.Empty;

    // Navigation properties
    public Court Court { get; set; } = null!;
    public CourtDepartment Department { get; set; } = null!;
    public ICollection<CaseTimelineEvent> TimelineEvents { get; set; } = new List<CaseTimelineEvent>();
}
