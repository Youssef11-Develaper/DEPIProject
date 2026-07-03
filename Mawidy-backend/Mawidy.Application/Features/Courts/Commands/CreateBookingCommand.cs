using MediatR;
using Mawidy.Application.DTOs.Courts;
using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

namespace Mawidy.Application.Features.Courts.Commands;

public record CreateBookingCommand : IRequest<BookingResultDto>
{
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? CaseNumber { get; set; }
    
    public Guid CourtId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid ServiceId { get; set; }
    
    public DateTime BookingDate { get; set; }
    public TimeSpan TimeSlot { get; set; }
    public string? Notes { get; set; }
    
    public bool WantsSms { get; set; }
    public bool WantsReminder { get; set; }
}


