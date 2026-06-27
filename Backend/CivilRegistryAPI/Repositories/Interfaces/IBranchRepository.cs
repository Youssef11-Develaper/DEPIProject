using CivilRegistryAPI.Models;

namespace CivilRegistryAPI.Repositories.Interfaces
{
    public interface IBranchRepository : IGenericRepository<Branch>
    {
        Task<IEnumerable<Branch>> GetByGovernorateAsync(int governorateId);
        Task<Branch?> GetWithDetailsAsync(int branchId);
        Task<IEnumerable<Branch>> GetAllWithDetailsAsync();
        Task<double> GetAverageRatingAsync(int branchId);
    }
}
