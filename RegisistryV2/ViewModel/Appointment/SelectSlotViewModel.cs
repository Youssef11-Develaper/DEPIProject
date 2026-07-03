namespace RegisistryV2.ViewModel.Appointment
{
    public class SelectSlotViewModel
    {
        public int BranchId { get; set; }
        public int ServiceTypeId { get; set; }
        public DateTime SelectedDate { get; set; }
        public string BranchName { get; set; }
        public string ServiceName { get; set; }
        public string? RequiredDocuments { get; set; }

        public bool IsAvailable { get; set; } = true;
        public string? UnavailabilityMessage { get; set; }

        public string? BusiestDayName { get; set; }
        public string? BusiestPeriod { get; set; }

        public List<SlotItemViewModel> Slots { get; set; } = new();

        public int? RescheduleAppointmentId { get; set; }
        public bool IsRescheduling => RescheduleAppointmentId.HasValue;
    }


}
