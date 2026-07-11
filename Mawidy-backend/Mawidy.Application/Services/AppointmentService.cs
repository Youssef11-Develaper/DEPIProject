using Microsoft.EntityFrameworkCore;
using Mawidy.Application.DTOs;
using Mawidy.Application.Interfaces;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

namespace Mawidy.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppDbContext _db;
    public AppointmentService(IAppDbContext db) => _db = db;

    public async Task<AppointmentCreateViewModel?> GetBookingFormAsync(int branchId, string? serviceKey = null)
    {
        if (branchId == 0) return null;
        var branch = await _db.Branches
            .Include(b => b.Operator).ThenInclude(o => o.Services)
            .FirstOrDefaultAsync(b => b.Id == branchId);
        if (branch is null) return null;

        return new AppointmentCreateViewModel
        {
            BranchId          = branchId,
            BranchName        = branch.NameAr,
            OperatorName      = branch.Operator.NameAr,
            ServiceKey        = serviceKey ?? string.Empty,
            AppointmentDate   = DateTime.Today.AddDays(1),
            AvailableServices = branch.Operator.Services.Select(s => new ServiceItemViewModel
            {
                Key = s.ServiceKey, Icon = s.Icon, NameAr = s.NameAr, EstimatedTime = s.EstimatedTime
            }).ToList()
        };
    }

    public async Task<AppointmentConfirmViewModel> CreateAppointmentAsync(AppointmentCreateViewModel vm)
    {
        if (!TimeSpan.TryParse(vm.AppointmentTimeSlot, out var timeSpan))
            timeSpan = new TimeSpan(9, 0, 0);

        var appointment = new Appointment
        {
            BranchId        = vm.BranchId,
            ServiceKey      = vm.ServiceKey,
            CustomerName    = vm.CustomerName,
            CustomerPhone   = vm.CustomerPhone,
            AppointmentDate = vm.AppointmentDate.Date,
            AppointmentTime = timeSpan,
            Status          = AppointmentStatus.Confirmed,
            Notes           = vm.Notes,
            UserId          = string.IsNullOrEmpty(vm.UserId) ? null : vm.UserId
        };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        var branch = await _db.Branches
            .Include(b => b.Operator).ThenInclude(o => o.Services)
            .FirstAsync(b => b.Id == vm.BranchId);

        var svcName = branch.Operator.Services
            .FirstOrDefault(s => s.ServiceKey == vm.ServiceKey)?.NameAr ?? vm.ServiceKey;

        return new AppointmentConfirmViewModel
        {
            AppointmentId   = appointment.Id,
            BranchName      = branch.NameAr,
            OperatorName    = branch.Operator.NameAr,
            ServiceName     = svcName,
            CustomerName    = vm.CustomerName,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = timeSpan.ToString(@"hh\:mm")
        };
    }

    public async Task<List<string>> GetAvailableSlotsAsync(int branchId, DateTime date)
    {
        var booked = await _db.Appointments
            .Where(a => a.BranchId == branchId
                     && a.AppointmentDate == date.Date
                     && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.AppointmentTime)
            .ToListAsync();

        var slots = new List<string>();
        int end = date.DayOfWeek == DayOfWeek.Thursday ? 17 * 60 : 21 * 60;

        for (int min = 9 * 60; min < end; min += 30)
        {
            if (date.DayOfWeek == DayOfWeek.Friday) break;
            var ts = new TimeSpan(0, min, 0);
            if (!booked.Contains(ts))
                slots.Add(ts.ToString(@"hh\:mm"));
        }
        return slots;
    }

    public async Task<QueueStatusViewModel> JoinVirtualQueueAsync(JoinQueueViewModel vm)
    {
        var maxPos = await _db.VirtualQueueEntries
            .Where(e => e.BranchId == vm.BranchId && e.IsActive)
            .MaxAsync(e => (int?)e.Position) ?? 0;

        var entry = new VirtualQueueEntry
        {
            BranchId      = vm.BranchId,
            CustomerName  = vm.CustomerName,
            CustomerPhone = vm.CustomerPhone,
            ServiceKey    = vm.ServiceKey,
            Position      = maxPos + 1,
            IsActive      = true
        };
        _db.VirtualQueueEntries.Add(entry);
        await _db.SaveChangesAsync();

        var branch = await _db.Branches.FindAsync(vm.BranchId);

        return new QueueStatusViewModel
        {
            EntryId              = entry.Id,
            BranchId             = vm.BranchId,
            BranchName           = branch?.NameAr ?? string.Empty,
            Position             = entry.Position,
            TotalAhead           = entry.Position - 1,
            EstimatedWaitMinutes = (entry.Position - 1) * 8
        };
    }

    public async Task LeaveQueueAsync(int entryId)
    {
        var entry = await _db.VirtualQueueEntries.FindAsync(entryId);
        if (entry is not null)
        {
            entry.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
