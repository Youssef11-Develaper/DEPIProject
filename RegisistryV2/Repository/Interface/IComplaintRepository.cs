using RegisistryV2.Models;
using static RegisistryV2.Repository.Interface.IGenericRepository;

namespace RegisistryV2.Repository.Interface
{
    public interface IComplaintRepository : IGenericRepository<Complaint>
    {
        Task<IEnumerable<Complaint>> GetUserComplaintsAsync(string userId);
        Task<IEnumerable<Complaint>> GetAllWithDetailsAsync(ComplaintStatus? status = null);
        Task<Complaint?> GetWithDetailsAsync(int id);
    }
}
