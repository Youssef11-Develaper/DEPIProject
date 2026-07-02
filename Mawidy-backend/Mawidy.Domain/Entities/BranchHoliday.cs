

namespace Mawidy.Domain.Entities
{
    public class BranchHoliday
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
