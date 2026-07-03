using Microsoft.AspNetCore.Identity;

namespace RegisistryV2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string NationalId { get; set; }
        public int? GovernorateId { get; set; }
        public Governorate Governorate { get; set; }
        public string? Area { get; set; }
        public int? BranchId { get; set; }       
        public Branch? Branch { get; set; }        
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
        public ICollection<Rating> Ratings { get; set; }
    }
}
