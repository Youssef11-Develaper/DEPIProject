using CivilRegistryAPI.Data;
using CivilRegistryAPI.Models;
using CivilRegistryAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CivilRegistryAPI.Repositories.Implementations
{
    public class RatingRepository : GenericRepository<Rating>, IRatingRepository
    {
        public RatingRepository(AppDbContext context) : base(context) { }

        public async Task<bool> HasRatedAsync(int appointmentId)
            => await _context.Ratings.AnyAsync(r => r.AppointmentId == appointmentId);

        public async Task<IEnumerable<Rating>> GetBranchRatingsAsync(int branchId)
            => await _context.Ratings
                .Include(r => r.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.ServiceType)
                .Where(r => r.BranchId == branchId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<double> GetBranchAverageAsync(int branchId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.BranchId == branchId)
                .ToListAsync();

            return ratings.Any() ? Math.Round(ratings.Average(r => r.Stars), 1) : 0;
        }

        public async Task<IEnumerable<Rating>> GetRecentPositiveRatingsAsync(int count = 4)
            => await _context.Ratings
                .Include(r => r.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.ServiceType)
                .Include(r => r.Branch)
                .Where(r => r.Stars >= 4 && r.Comment != null)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
    }
}
