namespace CivilRegistryAPI.DTOs.Appointments
{
    public class SlotDto
    {
        public TimeSpan Time { get; set; }
        public bool IsBooked { get; set; }
        public bool IsPeak { get; set; }
        public int BookedCount { get; set; }
        public int MaxCount { get; set; }
        public int RemainingCount => MaxCount - BookedCount;
    }
}
