using Mawidy.Application.DTOs.Appointments;

namespace Mawidy.Application.Interfaces
{
    public interface IAppointmentAvailabilityService
    {
        Task<AvailableSlotsDto> BuildSlotViewModelAsync(int branchId, int serviceTypeId, DateTime selectedDate);
    }
}
