using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.ViewModel.BranchAdmin;

namespace RegisistryV2.Controllers
{
    [Authorize(Roles = "BranchAdmin")]
    public class BranchAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchAdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // لوحة التحكم
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user?.BranchId == null) return NotFound();

            var today = DateTime.Today;
            var branchId = user.BranchId.Value;

            var viewModel = new BranchAdminDashboardViewModel
            {
                BranchName = user.Branch.Name,
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.BranchId == branchId
                        && a.AppointmentDate.Date == today),
                TodayCompleted = await _context.Appointments
                    .CountAsync(a => a.BranchId == branchId
                        && a.AppointmentDate.Date == today
                        && a.Status == AppointmentStatus.Completed),
                TodayCancelled = await _context.Appointments
                    .CountAsync(a => a.BranchId == branchId
                        && a.AppointmentDate.Date == today
                        && a.Status == AppointmentStatus.Cancelled),
                AverageRating = await _context.Ratings
                    .Where(r => r.BranchId == branchId)
                    .AverageAsync(r => (double?)r.Stars) ?? 0,
                TodayAppointmentsList = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.ServiceType)
                    .Where(a => a.BranchId == branchId
                        && a.AppointmentDate.Date == today)
                    .OrderBy(a => a.TimeSlot)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // تغيير حالة موعد
        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, AppointmentStatus status)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.BranchId == user.BranchId);

            if (appointment == null) return NotFound();

            appointment.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث حالة الموعد";
            return RedirectToAction("Index");
        }

        // إدارة مواعيد الفرع
        public async Task<IActionResult> Schedule()
        {
            var user = await _userManager.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user?.BranchId == null) return NotFound();

            var branchId = user.BranchId.Value;
            var branch = await _context.Branches.FindAsync(branchId);

            var schedules = await _context.BranchSchedules
                .Where(s => s.BranchId == branchId)
                .OrderBy(s => s.DayOfWeek)
                .ToListAsync();

            var holidays = await _context.BranchHolidays
                .Where(h => h.BranchId == branchId && h.Date >= DateTime.Today)
                .OrderBy(h => h.Date)
                .ToListAsync();

            var unavailabilities = await _context.ServiceUnavailabilities
                .Include(s => s.ServiceType)
                .Where(s => s.BranchId == branchId && s.Date >= DateTime.Today)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var vm = new BranchDetailsViewModel
            {
                Branch = branch,
                Schedules = schedules,
                Holidays = holidays,
                Unavailabilities = unavailabilities,
                Services = await _context.ServiceTypes.ToListAsync()
            };

            return View(vm);
        }

        // إضافة إجازة
        [HttpPost]
        public async Task<IActionResult> AddHoliday(DateTime date, string reason)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user?.BranchId == null) return NotFound();

            _context.BranchHolidays.Add(new BranchHoliday
            {
                BranchId = user.BranchId.Value,
                Date = date,
                Reason = reason
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إضافة الإجازة";
            return RedirectToAction("Schedule");
        }

        // تعطيل خدمة
        [HttpPost]
        public async Task<IActionResult> AddServiceUnavailability(int serviceTypeId,
            DateTime date, string reason)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user?.BranchId == null) return NotFound();

            _context.ServiceUnavailabilities.Add(new ServiceUnavailability
            {
                BranchId = user.BranchId.Value,
                ServiceTypeId = serviceTypeId,
                Date = date,
                Reason = reason
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تعطيل الخدمة";
            return RedirectToAction("Schedule");
        }
    }
}
