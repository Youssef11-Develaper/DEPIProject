using System.ComponentModel.DataAnnotations;

namespace RegisistryV2.Models
{
    public class Governorate
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public ICollection<Branch> Branches { get; set; }
    }
}
