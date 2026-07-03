namespace Maw3dyCare.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalHospitals { get; set; }  // عدد المستشفيات الكلي
        public int TotalBeds { get; set; }        // عدد السراير الكلي
        public int AvailableBeds { get; set; }    // عدد السراير المتاحة
        public int OccupiedBeds { get; set; }     // عدد السراير المشغولة

        // قائمة المستشفيات اللي هتتعرض في الكاردز في الأسفل
        public List<HospitalCardViewModel> Hospitals { get; set; } = new();
    }

    // بيانات كل مستشفى في الكارد
    public class HospitalCardViewModel
    {
        public int HospitalId { get; set; }    
        public string Name { get; set; }       
        public string City { get; set; }        
        public string Address { get; set; }    
        public bool IsActive { get; set; }      
        public int TotalBeds { get; set; }      
        public int AvailableBeds { get; set; } 


    }
}
