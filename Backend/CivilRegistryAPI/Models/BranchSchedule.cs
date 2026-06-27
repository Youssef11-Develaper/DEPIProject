namespace CivilRegistryAPI.Models
{
    public class BranchSchedule
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public TimeSpan PeakStartTime { get; set; }
        public TimeSpan PeakEndTime { get; set; }
        public int MaxAppointmentsPerSlot { get; set; }
    }
}
