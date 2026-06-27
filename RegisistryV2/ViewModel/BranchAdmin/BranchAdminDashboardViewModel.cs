namespace RegisistryV2.ViewModel.BranchAdmin
{
    public class BranchAdminDashboardViewModel
    {
        public string BranchName { get; set; }
        public int TodayAppointments { get; set; }
        public int TodayCompleted { get; set; }
        public int TodayCancelled { get; set; }
        public double AverageRating { get; set; }
        public List<RegisistryV2.Models.Appointment> TodayAppointmentsList { get; set; }
    }
}
