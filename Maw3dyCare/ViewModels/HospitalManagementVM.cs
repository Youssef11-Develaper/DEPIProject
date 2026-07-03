using System.ComponentModel.DataAnnotations;

namespace Maw3dyCare.ViewModels
{
    
    public class ReservationListItemVM
    {
        public int ReservationId { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string BedTypeName { get; set; }
        public string CaseDescription { get; set; }
        public string Status { get; set; }
        public int ETA { get; set; }  // بالدقائق
        public DateTime ReservedAt { get; set; }

        public int RemainingSeconds
        {
            get
            {
                var arrivalTime = ReservedAt.AddMinutes(ETA);
                var remaining = (arrivalTime - DateTime.Now).TotalSeconds;
                return remaining > 0 ? (int)remaining : 0;
            }
        }
    }

   
    public class ReservationDetailVM : ReservationListItemVM
    {
        public string HospitalName { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

   
    public class AddBedVM
    {
        [Required(ErrorMessage = "Bed number is required")]
        public string BedNumber { get; set; }

        [Required(ErrorMessage = "Bed type is required")]
        public int BedTypeId { get; set; }

        public string Status { get; set; } = "Available";
    }

  
    public class ManageBedItemVM
    {
        public int BedId { get; set; }
        public string BedNumber { get; set; }
        public string BedTypeName { get; set; }
        public int BedTypeId { get; set; }
        public string Status { get; set; }
        public string? AssignedPatient { get; set; }
        public string? PatientPhone { get; set; }
        public string? CaseDescription { get; set; }
    }

   
    public class HistoryItemVM
    {
        public int ReservationId { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string BedTypeName { get; set; }
        public string CaseDescription { get; set; }
        public string Status { get; set; } 
        public int ETA { get; set; }
        public DateTime ReservedAt { get; set; }
    }

  
    public class BlockedUserVM
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string? Reason { get; set; }
        public DateTime BlockedAt { get; set; }
    }

  
    public class BlockPhoneVM
    {
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^01[0125][0-9]{8}$",
            ErrorMessage = "Must start with 010/011/012/015")]
        public string Phone { get; set; }

        public string? Reason { get; set; }
    }
}