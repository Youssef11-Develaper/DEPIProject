using CivilRegistryAPI.Data;
using CivilRegistryAPI.Models;
using CivilRegistryAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CivilRegistryAPI.Repositories.Implementations
{
    public class BranchRepository : GenericRepository<Branch>, IBranchRepository
    {
        public BranchRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Branch>> GetByGovernorateAsync(int governorateId)
            => await _context.Branches
                .Include(b => b.Governorate)
                .Include(b => b.Schedules)
                .Where(b => b.GovernorateId == governorateId)
                .ToListAsync();

        public async Task<Branch?> GetWithDetailsAsync(int branchId)
            => await _context.Branches
                .Include(b => b.Governorate)
                .Include(b => b.Schedules)
                .Include(b => b.Holidays)
                .Include(b => b.ServiceUnavailabilities)
                    .ThenInclude(s => s.ServiceType)
                .FirstOrDefaultAsync(b => b.Id == branchId);

        public async Task<IEnumerable<Branch>> GetAllWithDetailsAsync()
            => await _context.Branches
                .Include(b => b.Governorate)
                .Include(b => b.Schedules)
                .ToListAsync();

        public async Task<double> GetAverageRatingAsync(int branchId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.BranchId == branchId)
                .ToListAsync();

            return ratings.Any() ? Math.Round(ratings.Average(r => r.Stars), 1) : 0;
        }
    }
}
