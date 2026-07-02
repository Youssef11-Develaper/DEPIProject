

namespace Mawidy.Application.DTOs.Appointments
{
    public class AvailableSlotsDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int ServiceTypeId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime SelectedDate { get; set; }
        public bool IsAvailable { get; set; }
        public string? UnavailabilityMessage { get; set; }
        public string? BusiestDayName { get; set; }
        public string? BusiestPeriod { get; set; }
        public List<SlotDto> Slots { get; set; } = new();
    }
}

