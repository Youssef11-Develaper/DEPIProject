namespace RegisistryV2.ViewModel.Appointment
{
    public class SelectBranchViewModel
    {
        public int ServiceTypeId { get; set; }
        public List<BranchListItemViewModel> Branches { get; set; } = new();
        public string? UserGovernorateName { get; set; }
        public string? UserArea { get; set; }
        public double MapCenterLat { get; set; }
        public double MapCenterLng { get; set; }
    }

    public class BranchListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string GovernorateName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int WorkingDaysCount { get; set; }
    }
}
