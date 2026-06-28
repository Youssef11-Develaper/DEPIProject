using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

namespace Mawidy.Application.Interfaces
{
    public interface IBranchRepository : IGenericRepository<Branch>
    {
        Task<IEnumerable<Branch>> GetByGovernorateAsync(int governorateId);
        Task<Branch?> GetWithDetailsAsync(int branchId);
        Task<IEnumerable<Branch>> GetAllWithDetailsAsync();
        Task<double> GetAverageRatingAsync(int branchId);
    }
}

