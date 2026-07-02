namespace Mawidy.Application.DTOs.Courts;

public class CourtStatisticsDto
{
    public int SessionsToday { get; set; }
    public string SessionsChangePercentage { get; set; } = string.Empty;
    public bool SessionsIsUp { get; set; }

    public int JudgmentsIssued { get; set; }
    public string JudgmentsChangePercentage { get; set; } = string.Empty;
    public bool JudgmentsIsUp { get; set; }

    public int PostponedCases { get; set; }
    public string PostponedChangePercentage { get; set; } = string.Empty;
    public bool PostponedIsUp { get; set; }

    public List<UpcomingSessionDto> UpcomingSessions { get; set; } = new();
    public List<NearestCourtDto> NearestCourts { get; set; } = new();
}

public class UpcomingSessionDto
{
    public string DateString { get; set; } = string.Empty;
    public int SessionsCount { get; set; }
    public string Status { get; set; } = string.Empty; // "متاح", "مزدحم"
}

public class NearestCourtDto
{
    public string Name { get; set; } = string.Empty;
    public string DistanceString { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
