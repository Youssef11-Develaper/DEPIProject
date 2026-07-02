using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

namespace Mawidy.Application.Interfaces
{
    public interface IComplaintRepository : IGenericRepository<Complaint>
    {
        Task<IEnumerable<Complaint>> GetUserComplaintsAsync(string userId);
        Task<IEnumerable<Complaint>> GetAllWithDetailsAsync(ComplaintStatus? status = null);
        Task<Complaint?> GetWithDetailsAsync(int id);
    }
}

