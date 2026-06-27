using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;
using RegisistryV2.ViewModel.Complaint;

namespace RegisistryV2.Controllers
{
    [Authorize]
    public class ComplaintController : Controller
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintController(
            IComplaintRepository complaintRepository,
            IAppointmentRepository appointmentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _complaintRepository = complaintRepository;
            _appointmentRepository = appointmentRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var complaints = await _complaintRepository.GetUserComplaintsAsync(userId);
            return View(complaints);
        }

        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _appointmentRepository.GetUserAppointmentsAsync(userId);
            var completedAppointments = appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .ToList();

            if (!completedAppointments.Any())
            {
                TempData["Error"] = "لازم يكون عندك موعد مكتمل عشان تقدر تقدم شكوى";
                return RedirectToAction("MyAppointments", "Appointment");
            }

            ViewBag.Appointments = completedAppointments;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ComplaintViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);

            var complaint = new Complaint
            {
                UserId = userId,
                AppointmentId = model.AppointmentId == 0 ? null : model.AppointmentId,
                Title = model.Title,
                Description = model.Description,
                Status = ComplaintStatus.Submitted,
                CreatedAt = DateTime.Now
            };

            await _complaintRepository.AddAsync(complaint);
            await _complaintRepository.SaveChangesAsync();

            TempData["Success"] = "تم إرسال شكواك بنجاح";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var complaint = await _complaintRepository.GetWithDetailsAsync(id);

            if (complaint == null || complaint.UserId != userId)
                return NotFound();

            return View(complaint);
        }
    }
}
