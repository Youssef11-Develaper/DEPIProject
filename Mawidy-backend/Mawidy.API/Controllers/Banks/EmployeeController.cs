
using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.API.Hubs.Banks;
using Mawidy.API.Hubs.Hospitals;
using Mawidy.Application.Banks.Services;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Mawidy.Domain.Entities;


namespace Mawidy.API.Controllers.Banks
{
    public class EmployeeController : BaseController
    {
        private readonly LocalizationService _localizer;

        public EmployeeController(Mawidy.Infrastructure.Persistence.AppDbContext context, LocalizationService localizer, IConfiguration configuration)
            : base(context, configuration)
        {
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            var authResult = CheckAuthorization();
            if (authResult != null) return authResult;

            // Check if current user is employee
            var currentUser = _context.Users.Include(u => u.Branch).FirstOrDefault(u => u.Id == CurrentUserId);
            if (currentUser == null || !currentUser.IsBankEmployee)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Load branches for filter dropdown. If employee is tied to a branch, they can only filter that branch.
            var branches = _context.Branches.Where(b => b.SystemType == Mawidy.Domain.Enums.SystemType.Bank).OrderBy(b => b.NameEn).AsQueryable();
            if (currentUser.BranchId.HasValue)
            {
                branches = branches.Where(b => b.Id == currentUser.BranchId.Value);
            }
            ViewBag.Branches = branches.ToList();

            // Fetch appointments. Filter by employee's branch if configured.
            var query = _context.Appointments.Where(a => a.SystemType == Mawidy.Domain.Enums.SystemType.Bank)
                .Include(a => a.Branch)
                .Include(a => a.User)
                .AsQueryable();

            if (currentUser.BranchId.HasValue)
            {
                query = query.Where(a => a.BranchId == currentUser.BranchId.Value);
                ViewBag.EmployeeBranchId = currentUser.BranchId.Value;
                ViewBag.EmployeeBranchName = _localizer.GetCurrentCulture() == "ar" ? currentUser.Branch?.NameAr : currentUser.Branch?.NameEn;
            }

            var appointments = query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            // Calculate statistics
            ViewBag.TotalCount = appointments.Count;
            ViewBag.PendingCount = appointments.Count(a => a.Status == Mawidy.Domain.Enums.AppointmentStatus.Confirmed);
            ViewBag.CompletedCount = appointments.Count(a => a.Status == Mawidy.Domain.Enums.AppointmentStatus.Completed);
            ViewBag.CancelledCount = appointments.Count(a => a.Status == Mawidy.Domain.Enums.AppointmentStatus.Cancelled);

            return View(appointments);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status, string? remarks)
        {
            // Check if current user is employee
            var currentUser = CurrentUser;
            if (currentUser == null || !IsBankEmployee)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            if (string.IsNullOrEmpty(status))
            {
                return Json(new { success = false, message = "Status is required." });
            }

            // Normalize status to lowercase
            status = status.ToLower();
            if (status != "confirmed" && status != "completed" && status != "cancelled")
            {
                return Json(new { success = false, message = "Invalid status value." });
            }

            var appointment = _context.Appointments.Where(a => a.SystemType == Mawidy.Domain.Enums.SystemType.Bank).FirstOrDefault(a => a.Id == id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found." });
            }

            // Verify if the employee is restricted to a branch and the appointment belongs to it
            if (currentUser.BranchId.HasValue && appointment.BranchId != currentUser.BranchId.Value)
            {
                return Json(new { success = false, message = "Unauthorized to manage appointments for other branches." });
            }

            Enum.TryParse<Mawidy.Domain.Enums.AppointmentStatus>(status, true, out var parsedStatus); appointment.Status = parsedStatus;
            appointment.EmployeeRemarks = remarks;
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
