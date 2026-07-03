using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mawidy.Application.DTOs
{
    public class AdminV2UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Role { get; set; } = string.Empty;
        public string GovernorateName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int AppointmentsCount { get; set; }
    }

    public class AdminV2UserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Role { get; set; } = string.Empty;
        public string GovernorateName { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public List<AdminV2UserAppointmentDto> Appointments { get; set; } = new();
    }

    public class AdminV2UserAppointmentDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class AdminV2ResetPasswordDto
    {
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AdminV2UserListResponse
    {
        public IEnumerable<AdminV2UserDto> Users { get; set; } = Array.Empty<AdminV2UserDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateOperatorServiceDto
    {
        public int OperatorId { get; set; }
        public string ServiceKey { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
    }
}
