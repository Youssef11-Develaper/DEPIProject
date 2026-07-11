namespace Mawidy.Application.Hospitals.ViewModels
{
    public class HospitalsPageVM
    {
        public List<HospitalListItemVM> Hospitals { get; set; } = new();
        public string? SearchTerm { get; set; }
    }

    public class HospitalListItemVM
    {
        public int HospitalId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int OccupiedBeds { get; set; }
    }

    public class HospitalDetailsVM
    {
        public int HospitalId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BedTypeStatusVM> BedTypeStatuses { get; set; } = new();
    }

    public class BedTypeStatusVM
    {
        public string TypeName { get; set; }
        public int Total { get; set; }
        public int Available { get; set; }
        public int Occupied { get; set; }
    }
}
