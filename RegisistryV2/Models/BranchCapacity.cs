using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegisistryV2.Models
{
    public class BranchCapacity
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int CurrentCount { get; set; }
        public int MaxCapacity { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
