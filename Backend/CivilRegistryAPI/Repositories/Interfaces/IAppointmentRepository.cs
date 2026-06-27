using CivilRegistryAPI.Models;

namespace CivilRegistryAPI.Repositories.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetUserAppointmentsAsync(string userId);
        Task<IEnumerable<Appointment>> GetByBranchAndDateAsync(int branchId, DateTime date);
        Task<Appointment?> GetWithDetailsAsync(int id);
        Task<bool> HasAppointmentSameDayAsync(string userId, DateTime date, int serviceTypeId);
        Task<int> GetBookedCountAsync(int branchId, DateTime date, TimeSpan timeSlot);
        Task<int> GetQueueNumberAsync(int branchId, DateTime date, int serviceTypeId, TimeSpan timeSlot);
        Task<IEnumerable<Appointment>> GetPendingReminderAppointmentsAsync(DateTime date);
    }
}
