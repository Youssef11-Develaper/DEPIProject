namespace Maw3dyCare.ViewModels
{
    public class HospitalDashboardVM
    {
        public string HospitalName { get; set; }
        public string City { get; set; }
        public bool IsActive { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int PendingCount { get; set; }  
        public List<BedTypeSummary> AvailableByType { get; set; } = new();
        public List<BedTypeSummary> OccupiedByType { get; set; } = new();
    }



    public class BedTypeSummary
    {
        public string TypeName { get; set; }  
        public int Count { get; set; }
    }

}
    
