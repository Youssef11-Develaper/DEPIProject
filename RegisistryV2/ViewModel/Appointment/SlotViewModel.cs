namespace RegisistryV2.ViewModel.Appointment
{
    public class SlotViewModel
    {
        public int BranchId { get; set; }
        public int ServiceTypeId { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<SlotItemViewModel> Slots { get; set; } = new();
    }
}
