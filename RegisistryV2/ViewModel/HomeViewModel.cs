namespace RegisistryV2.ViewModel
{
    public class HomeViewModel
    {
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalCompletedAppointments { get; set; }
        public List<RecentRatingViewModel> RecentRatings { get; set; } = new();
    }

    public class RecentRatingViewModel
    {
        public string UserName { get; set; }
        public int Stars { get; set; }
        public string? Comment { get; set; }
        public string ServiceName { get; set; }
        public string BranchName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
