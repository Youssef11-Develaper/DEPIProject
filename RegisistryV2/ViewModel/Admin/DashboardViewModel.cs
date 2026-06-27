namespace RegisistryV2.ViewModel.Admin
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBranches { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalComplaints { get; set; }
        public int TodayAppointments { get; set; }
        public int TodayCompleted { get; set; }
        public int TodayCancelled { get; set; }
        public int PendingComplaints { get; set; }
        public List<BranchStatsViewModel> TopBranches { get; set; }
        public List<DailyAppointmentsViewModel> WeeklyAppointments { get; set; }
        public List<Models.Complaint> RecentComplaints { get; set; }

        // جديد
        public List<ServiceStatsViewModel> TopServices { get; set; } = new();
        public List<GovernorateStatsViewModel> GovernorateDistribution { get; set; } = new();
        public double CancellationRate { get; set; }
    }

    public class ServiceStatsViewModel
    {
        public string ServiceName { get; set; }
        public int TotalAppointments { get; set; }
        public double Percentage { get; set; }
    }

    public class GovernorateStatsViewModel
    {
        public string GovernorateName { get; set; }
        public int TotalAppointments { get; set; }
    }
}
