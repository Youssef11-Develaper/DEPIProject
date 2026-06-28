using TelecomBranches.Application.DTOs;

namespace TelecomBranches.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentCreateViewModel?> GetBookingFormAsync(int branchId, string? serviceKey = null);
    Task<AppointmentConfirmViewModel> CreateAppointmentAsync(AppointmentCreateViewModel vm);
    Task<List<string>>                GetAvailableSlotsAsync(int branchId, DateTime date);
    Task<QueueStatusViewModel>        JoinVirtualQueueAsync(JoinQueueViewModel vm);
    Task                              LeaveQueueAsync(int entryId);
}
