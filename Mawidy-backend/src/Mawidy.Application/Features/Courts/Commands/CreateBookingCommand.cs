using MediatR;
using Mawidy.Application.DTOs.Courts;

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

using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingResultDto>
{
    private readonly IApplicationDbContext _context;

    public CreateBookingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResultDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // Generate QR Code string
        string qrCode = $"⚖️-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        int queueNumber = new Random().Next(50, 100);

        var booking = new Booking
        {
            UserId = request.UserId,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            NationalId = request.NationalId,
            CaseNumber = request.CaseNumber,
            CourtId = request.CourtId,
            DepartmentId = request.DepartmentId,
            ServiceId = request.ServiceId,
            BookingDate = request.BookingDate,
            TimeSlot = request.TimeSlot,
            Notes = request.Notes,
            WantsSms = request.WantsSms,
            WantsReminder = request.WantsReminder,
            QrCode = qrCode,
            QueueNumber = queueNumber,
            Status = BookingStatus.Confirmed
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        // Here we could publish an event to send an SMS if request.WantsSms == true

        return new BookingResultDto
        {
            BookingId = booking.Id,
            QrCode = booking.QrCode,
            QueueNumber = booking.QueueNumber,
            DateString = request.BookingDate.ToString("dd MMMM yyyy"),
            TimeString = request.TimeSlot.ToString(@"hh\:mm"),
            CourtName = "المحكمة المحددة", // Usually fetched from _context.Courts
            ServiceName = "الخدمة المحددة"
        };
    }
}
