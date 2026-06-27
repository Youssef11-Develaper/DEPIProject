namespace RegisistryV2.Models
{
    public class ServiceUnavailability
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }

        public int ServiceTypeId { get; set; }
        public ServiceType ServiceType { get; set; }

        public int BranchId { get; set; }
        public Branch Branch { get; set; }
    }
}
