using CivilRegistryAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace CivilRegistryAPI.DTOs.Complaints
{
    public class RespondComplaintDto
    {
        [Required]
        public string AdminResponse { get; set; } = string.Empty;

        [Required]
        public ComplaintStatus Status { get; set; }
    }
}
