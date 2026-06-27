using RegisistryV2.Models;
using static RegisistryV2.Repository.Interface.IGenericRepository;

namespace RegisistryV2.Repository.Interface
{
    public interface IBranchRepository : IGenericRepository<Branch>
    {
        Task<IEnumerable<Branch>> GetByGovernorateAsync(int governorateId);
        Task<Branch?> GetWithSchedulesAsync(int branchId);
        Task<IEnumerable<Branch>> GetAllWithDetailsAsync();
    }
}
