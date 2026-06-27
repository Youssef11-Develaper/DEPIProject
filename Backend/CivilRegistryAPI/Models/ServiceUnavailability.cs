namespace CivilRegistryAPI.Models
{
    public class ServiceUnavailability
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public int ServiceTypeId { get; set; }
        public ServiceType ServiceType { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
