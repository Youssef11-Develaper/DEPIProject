using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.ViewModel;

namespace RegisistryV2.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users
                .Include(u => u.Governorate)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null) return NotFound();

            var appointments = await _context.Appointments
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                GovernorateId = user.GovernorateId ?? 0,
                Area = user.Area,
                Email = user.Email,
                NationalId = user.NationalId,
                TotalAppointments = appointments.Count,
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                TotalComplaints = await _context.Complaints.CountAsync(c => c.UserId == user.Id)
            };

            ViewBag.Governorates = await _context.Governorates.ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Governorates = await _context.Governorates.ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.GovernorateId = model.GovernorateId;
            user.Area = model.Area;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "تم تحديث بياناتك";
            return RedirectToAction("Index");
        }
    }
}
