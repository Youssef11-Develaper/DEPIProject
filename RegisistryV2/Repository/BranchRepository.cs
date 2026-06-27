using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;

namespace RegisistryV2.Repository
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

        public async Task<Branch?> GetWithSchedulesAsync(int branchId)
            => await _context.Branches
                .Include(b => b.Schedules)
                .FirstOrDefaultAsync(b => b.Id == branchId);

        public async Task<IEnumerable<Branch>> GetAllWithDetailsAsync()
            => await _context.Branches
                .Include(b => b.Governorate)
                .Include(b => b.Schedules)
                .ToListAsync();
    }
}
