using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;

namespace Mawidy.Application.Interfaces
{
    public interface IRatingRepository : IGenericRepository<Rating>
    {
        Task<bool> HasRatedAsync(int appointmentId);
        Task<IEnumerable<Rating>> GetBranchRatingsAsync(int branchId);
        Task<double> GetBranchAverageAsync(int branchId);
        Task<IEnumerable<Rating>> GetRecentPositiveRatingsAsync(int count = 4);
    }
}

