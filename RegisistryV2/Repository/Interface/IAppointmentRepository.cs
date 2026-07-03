using RegisistryV2.Models;
using static RegisistryV2.Repository.Interface.IGenericRepository;

namespace RegisistryV2.Repository.Interface
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetUserAppointmentsAsync(string userId);
        Task<IEnumerable<Appointment>> GetByBranchAndDateAsync(int branchId, DateTime date);
        Task<bool> HasAppointmentSameDayAsync(string userId, DateTime date, int serviceTypeId);
        Task<int> GetBookedCountAsync(int branchId, DateTime date, TimeSpan timeSlot);
        Task<Appointment?> GetWithDetailsAsync(int id);
        Task<int> GetQueueNumberAsync(int branchId, DateTime date, int serviceTypeId, TimeSpan timeSlot);
        Task<IEnumerable<ServiceType>> GetAllServicesAsync();
    }
}
