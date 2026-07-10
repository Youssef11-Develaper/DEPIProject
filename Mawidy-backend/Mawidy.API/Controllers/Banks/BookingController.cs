
using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.API.Hubs.Banks;
using Mawidy.API.Hubs.Hospitals;
using Mawidy.Application.Banks.Services;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Mawidy.Domain.Entities;


namespace Mawidy.API.Controllers.Banks
{
    public class BookingController : BaseController
    {
        private readonly IHubContext<BookingHub> _hubContext;
        private static readonly object _bookingLock = new object();

        public BookingController(Mawidy.Infrastructure.Persistence.AppDbContext context, IConfiguration configuration, IHubContext<BookingHub> hubContext)
            : base(context, configuration)
        {
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var authResult = CheckAuthorization();
            if (authResult != null) return authResult;

            // Get user info
            var user = CurrentUser;
            if (user != null && user.IsBankEmployee)
            {
                return RedirectToAction("Index", "Employee");
            }

            // Pass branches to the view
            var branches = _context.Branches.Where(b => b.SystemType == Mawidy.Domain.Enums.SystemType.Bank).OrderBy(b => b.DistanceKm).ToList();
            ViewBag.Branches = branches;

            // Pass services to the view
            var services = _context.BankServices.ToList();
            ViewBag.Services = services;

            ViewBag.UserName = user?.FirstName ?? "Adham Emad";
            ViewBag.UserCity = user?.City ?? string.Empty;
            ViewBag.UserPhone = user?.PhoneNumber ?? string.Empty;
            ViewBag.UserAddress = user?.Address ?? string.Empty;

            return View();
        }

        [HttpPost]
        public IActionResult Confirm([FromBody] BookingDto dto)
        {
            if (CurrentUser == null)
            {
                return Json(new { success = false, message = "Authentication required. Please log in." });
            }

            if (dto == null || string.IsNullOrEmpty(dto.Service) || dto.BranchId <= 0 || string.IsNullOrEmpty(dto.Date) || string.IsNullOrEmpty(dto.Time))
            {
                return Json(new { success = false, message = "Invalid booking details." });
            }

            // Verify branch exists
            var branchExists = _context.Branches.Where(b => b.SystemType == Mawidy.Domain.Enums.SystemType.Bank).Any(b => b.Id == dto.BranchId);
            if (!branchExists)
            {
                return Json(new { success = false, message = "Selected branch does not exist." });
            }

            Appointment appointment;

            lock (_bookingLock)
            {
                // Concurrency: Check if slot is already booked (and status is not cancelled)
                var slotTaken = _context.Appointments.Where(a => a.SystemType == Mawidy.Domain.Enums.SystemType.Bank).Any(a => 
                    a.BranchId == dto.BranchId && 
                    a.Date == dto.Date && 
                    a.Time == dto.Time && 
                    a.Status != Mawidy.Domain.Enums.AppointmentStatus.Cancelled);

                if (slotTaken)
                {
                    return Json(new { success = false, message = "This slot is already booked by another customer. Please choose a different slot." });
                }

                // Create appointment
                appointment = new Appointment
                {
                    Service = dto.Service,
                    BranchId = dto.BranchId,
                    Date = dto.Date,
                    Time = dto.Time,
                    Notes = dto.Notes,
                    Status = Mawidy.Domain.Enums.AppointmentStatus.Confirmed,
                    CreatedAt = DateTime.UtcNow,
                    UserId = CurrentUserId
                };

                _context.Appointments.Add(appointment);
                _context.SaveChanges();
            }

            // Real-time: retrieve complete details (with branch names & user info) to broadcast
            var createdAppointment = _context.Appointments.Where(a => a.SystemType == Mawidy.Domain.Enums.SystemType.Bank)
                .Include(a => a.Branch)
                .Include(a => a.User)
                .FirstOrDefault(a => a.Id == appointment.Id);

            if (createdAppointment != null)
            {
                _hubContext.Clients.All.SendAsync("NewBookingReceived", new
                {
                    id = createdAppointment.Id,
                    service = createdAppointment.Service,
                    branchId = createdAppointment.BranchId,
                    branchNameEn = createdAppointment.Branch?.NameEn ?? string.Empty,
                    branchNameAr = createdAppointment.Branch?.NameAr ?? string.Empty,
                    date = createdAppointment.Date,
                    time = createdAppointment.Time,
                    notes = createdAppointment.Notes ?? string.Empty,
                    status = createdAppointment.Status,
                    userName = createdAppointment.User?.FirstName ?? string.Empty,
                    userEmail = createdAppointment.User?.Email ?? string.Empty,
                    userPhone = createdAppointment.User?.PhoneNumber ?? string.Empty,
                    userCity = createdAppointment.User?.City ?? string.Empty,
                    userAddress = createdAppointment.User?.Address ?? string.Empty,
                    createdAt = createdAppointment.CreatedAt.ToString("o")
                });
            }

            return Json(new { success = true, appointmentId = appointment.Id });
        }
    }

    public class BookingDto
    {
        public string Service { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
