
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
    public class DashboardController : BaseController
    {
        public DashboardController(Mawidy.Infrastructure.Persistence.AppDbContext context, IConfiguration configuration)
            : base(context, configuration)
        {
        }

        public IActionResult Index()
        {
            var authResult = CheckAuthorization();
            if (authResult != null) return authResult;

            var user = CurrentUser;
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (user.IsBankEmployee)
            {
                return RedirectToAction("Index", "Employee");
            }

            // Fetch appointments
            var appointments = _context.Appointments.Where(a => a.SystemType == Mawidy.Domain.Enums.SystemType.Bank)
                .Where(a => a.UserId == CurrentUserId)
                .Include(a => a.Branch)
                .ToList();

            ViewBag.UserName = user.FullName;
            ViewBag.UserEmail = user.Email;

            return View(appointments);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            if (CurrentUser == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Authentication required." });
                }
                return RedirectToAction("Login", "Home");
            }

            var appointment = _context.Appointments.Where(a => a.SystemType == Mawidy.Domain.Enums.SystemType.Bank).FirstOrDefault(a => a.Id == id && a.UserId == CurrentUserId);
            if (appointment != null)
            {
                appointment.Status = Mawidy.Domain.Enums.AppointmentStatus.Cancelled;
                _context.SaveChanges();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                TempData["Message"] = "bookingCancelled";
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            return NotFound();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var authResult = CheckAuthorization();
            if (authResult != null) return authResult;

            var user = CurrentUser;
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var cities = _context.Branches.Where(b => b.SystemType == Mawidy.Domain.Enums.SystemType.Bank)
                .Select(b => new { b.CityEn, b.CityAr })
                .AsEnumerable()
                .GroupBy(c => c.CityEn)
                .Select(g => g.First())
                .ToList();
            ViewBag.Cities = cities;

            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(string name, string email, string phone, string city, string address)
        {
            if (CurrentUser == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Authentication required." });
                }
                return RedirectToAction("Login", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == CurrentUserId);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = name ?? string.Empty;
            user.Email = email ?? string.Empty;
            user.PhoneNumber = phone ?? string.Empty;
            user.City = city ?? string.Empty;
            user.Address = address ?? string.Empty;

            _context.SaveChanges();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            TempData["Message"] = "profileUpdated";
            return RedirectToAction(nameof(Profile));
        }
    }
}
