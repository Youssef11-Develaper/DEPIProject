using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;

namespace RegisistryV2.Repository
{
    public class ComplaintRepository : GenericRepository<Complaint>, IComplaintRepository
    {
        public ComplaintRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Complaint>> GetUserComplaintsAsync(string userId)
            => await _context.Complaints
                .Include(c => c.Appointment)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Complaint>> GetAllWithDetailsAsync(ComplaintStatus? status = null)
        {
            var query = _context.Complaints
                .Include(c => c.User)
                .Include(c => c.Appointment)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(c => c.Status == status);

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Complaint?> GetWithDetailsAsync(int id)
            => await _context.Complaints
                .Include(c => c.Appointment)
                .FirstOrDefaultAsync(c => c.Id == id);
    }
}
