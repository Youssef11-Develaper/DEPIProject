using CivilRegistryAPI.Data;
using CivilRegistryAPI.Models;
using CivilRegistryAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CivilRegistryAPI.Repositories.Implementations
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Appointment>> GetUserAppointmentsAsync(string userId)
            => await _context.Appointments
                .Include(a => a.Branch)
                .Include(a => a.ServiceType)
                .Include(a => a.Rating)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByBranchAndDateAsync(int branchId, DateTime date)
            => await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.ServiceType)
                .Where(a => a.BranchId == branchId
                    && a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.TimeSlot)
                .ToListAsync();

        public async Task<Appointment?> GetWithDetailsAsync(int id)
            => await _context.Appointments
                .Include(a => a.Branch)
                .Include(a => a.ServiceType)
                .Include(a => a.User)
                .Include(a => a.Rating)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<bool> HasAppointmentSameDayAsync(string userId, DateTime date, int serviceTypeId)
            => await _context.Appointments
                .AnyAsync(a => a.UserId == userId
                    && a.AppointmentDate.Date == date.Date
                    && a.ServiceTypeId == serviceTypeId
                    && a.Status != AppointmentStatus.Cancelled);

        public async Task<int> GetBookedCountAsync(int branchId, DateTime date, TimeSpan timeSlot)
            => await _context.Appointments
                .CountAsync(a => a.BranchId == branchId
                    && a.AppointmentDate.Date == date.Date
                    && a.TimeSlot == timeSlot
                    && a.Status != AppointmentStatus.Cancelled);

        public async Task<int> GetQueueNumberAsync(int branchId, DateTime date,
            int serviceTypeId, TimeSpan timeSlot)
            => await _context.Appointments
                .CountAsync(a => a.BranchId == branchId
                    && a.AppointmentDate.Date == date.Date
                    && a.ServiceTypeId == serviceTypeId
                    && a.Status != AppointmentStatus.Cancelled
                    && a.TimeSlot <= timeSlot);

        public async Task<IEnumerable<Appointment>> GetPendingReminderAppointmentsAsync(DateTime date)
            => await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Branch)
                .Include(a => a.ServiceType)
                .Where(a => a.AppointmentDate.Date == date.Date
                    && a.Status == AppointmentStatus.Confirmed
                    && !a.IsNotified)
                .ToListAsync();
    }
}
