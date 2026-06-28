

namespace Mawidy.Application.DTOs.Branches
{
    public class BranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GovernorateId { get; set; }
        public string GovernorateName { get; set; } = string.Empty;
        public int WorkingDaysCount { get; set; }
        public double AverageRating { get; set; }
        public List<ScheduleDto> Schedules { get; set; } = new();
    }
}

