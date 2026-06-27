namespace RegisistryV2.ViewModel.Appointment
{
    public class SlotItemViewModel
    {
        public TimeSpan Time { get; set; }
        public bool IsBooked { get; set; }
        public bool IsPeak { get; set; }
        public int BookedCount { get; set; }
        public int MaxCount { get; set; }
        public int RemainingCount => MaxCount - BookedCount;
    }
}
