namespace RegisistryV2.Models
{
    public class BranchHoliday
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }

        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
