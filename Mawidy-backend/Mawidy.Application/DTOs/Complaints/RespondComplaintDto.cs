using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mawidy.Application.DTOs.Complaints
{
    public class RespondComplaintDto
    {
        [Required]
        public string AdminResponse { get; set; } = string.Empty;

        [Required]
        public ComplaintStatus Status { get; set; }
    }
}

