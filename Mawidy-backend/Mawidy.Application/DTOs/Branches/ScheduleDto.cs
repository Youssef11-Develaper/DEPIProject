

namespace Mawidy.Application.DTOs.Branches
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public string DayName { get; set; } = string.Empty;
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public TimeSpan PeakStartTime { get; set; }
        public TimeSpan PeakEndTime { get; set; }
        public int MaxAppointmentsPerSlot { get; set; }
    }
}

