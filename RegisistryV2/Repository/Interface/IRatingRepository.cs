using RegisistryV2.Models;
using static RegisistryV2.Repository.Interface.IGenericRepository;

namespace RegisistryV2.Repository.Interface
{
    public interface IRatingRepository : IGenericRepository<Rating>
    {
        Task<bool> HasRatedAsync(int appointmentId);
        Task<IEnumerable<Rating>> GetBranchRatingsAsync(int branchId);
        Task<double> GetBranchAverageAsync(int branchId);
        Task<IEnumerable<Rating>> GetRecentPositiveRatingsAsync(int count = 4);
    }
}
