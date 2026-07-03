using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;
using RegisistryV2.Services;
using RegisistryV2.ViewModel.Appointment;

namespace RegisistryV2.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly QRService _qrService;
        private readonly AppointmentAvailabilityService _availabilityService;

        public AppointmentController(
            IAppointmentRepository appointmentRepository,
            IBranchRepository branchRepository,
            UserManager<ApplicationUser> userManager,
            EmailService emailService,
            QRService qrService,
            AppointmentAvailabilityService availabilityService)
        {
            _appointmentRepository = appointmentRepository;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _emailService = emailService;
            _qrService = qrService;
            _availabilityService = availabilityService;
        }

        // Step 1
        public async Task<IActionResult> SelectService()
        {
            var services = await _appointmentRepository.GetAllServicesAsync();
            return View(services);
        }

        // Step 2
        public async Task<IActionResult> SelectBranch(int serviceTypeId)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.Users
                .Include(u => u.Governorate)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var branches = user?.GovernorateId != null
                ? await _branchRepository.GetByGovernorateAsync(user.GovernorateId.Value)
                : await _branchRepository.GetAllWithDetailsAsync();

            var viewModel = new SelectBranchViewModel
            {
                ServiceTypeId = serviceTypeId,
                UserGovernorateName = user?.Governorate?.Name,
                UserArea = user?.Area,
                MapCenterLat = user?.Governorate?.CenterLatitude ?? 26.8206,
                MapCenterLng = user?.Governorate?.CenterLongitude ?? 30.8025,
                Branches = branches.Select(b => new BranchListItemViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    GovernorateName = b.Governorate.Name,
                    Latitude = b.Latitude,
                    Longitude = b.Longitude,
                    WorkingDaysCount = b.Schedules.Count
                }).ToList()
            };

            return View(viewModel);
        }

        // Step 3
        public async Task<IActionResult> SelectSlot(int serviceTypeId, int branchId, DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var viewModel = await _availabilityService
                .BuildSlotViewModelAsync(branchId, serviceTypeId, selectedDate);
            return View(viewModel);
        }

        // Confirm
        [HttpPost]
        public async Task<IActionResult> Confirm(int branchId, int serviceTypeId,
            DateTime date, TimeSpan timeSlot)
        {
            var userId = _userManager.GetUserId(User);

            var hasAppointment = await _appointmentRepository
                .HasAppointmentSameDayAsync(userId, date, serviceTypeId);

            if (hasAppointment)
            {
                TempData["Error"] = "حجزت الخدمة دي قبل كده في نفس اليوم";
                return RedirectToAction("SelectSlot", new { serviceTypeId, branchId, date });
            }

            var appointment = new Appointment
            {
                UserId = userId,
                BranchId = branchId,
                ServiceTypeId = serviceTypeId,
                AppointmentDate = date,
                TimeSlot = timeSlot,
                Status = AppointmentStatus.Confirmed,
                CreatedAt = DateTime.Now
            };

            await _appointmentRepository.AddAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            // إيميل التأكيد
            var fullAppointment = await _appointmentRepository.GetWithDetailsAsync(appointment.Id);
            var user = await _userManager.FindByIdAsync(userId);

            try
            {
                await _emailService.SendAppointmentConfirmationAsync(user, fullAppointment);
            }
            catch { }

            return RedirectToAction("MyAppointments");
        }

        // مواعيدي
        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _appointmentRepository.GetUserAppointmentsAsync(userId);
            return View(appointments);
        }

        // إلغاء
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null || appointment.UserId != userId)
                return NotFound();

            appointment.Status = AppointmentStatus.Cancelled;
            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return RedirectToAction("MyAppointments");
        }

        // تأجيل
        public async Task<IActionResult> Reschedule(int id, DateTime? date)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null || appointment.UserId != userId)
                return NotFound();

            if (appointment.Status != AppointmentStatus.Confirmed
                && appointment.Status != AppointmentStatus.Pending)
            {
                TempData["Error"] = "مش تقدر تأجل الموعد ده";
                return RedirectToAction("MyAppointments");
            }

            var selectedDate = date ?? DateTime.Today;
            var viewModel = await _availabilityService
                .BuildSlotViewModelAsync(appointment.BranchId, appointment.ServiceTypeId, selectedDate);

            viewModel.RescheduleAppointmentId = appointment.Id;
            return View("Reschedule", viewModel);
        }

        // تأكيد التأجيل
        [HttpPost]
        public async Task<IActionResult> ConfirmReschedule(int appointmentId,
            DateTime date, TimeSpan timeSlot)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

            if (appointment == null || appointment.UserId != userId)
                return NotFound();

            var bookedCount = await _appointmentRepository
                .GetBookedCountAsync(appointment.BranchId, date, timeSlot);

            var branch = await _branchRepository.GetWithSchedulesAsync(appointment.BranchId);
            var schedule = branch?.Schedules
                .FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek);

            if (schedule == null || bookedCount >= schedule.MaxAppointmentsPerSlot)
            {
                TempData["Error"] = "الموعد ده مش متاح، اختار موعد تاني";
                return RedirectToAction("Reschedule", new { id = appointmentId, date });
            }

            appointment.AppointmentDate = date;
            appointment.TimeSlot = timeSlot;
            appointment.Status = AppointmentStatus.Confirmed;
            appointment.IsNotified = false;

            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            TempData["Success"] = "تم تأجيل موعدك بنجاح";
            return RedirectToAction("MyAppointments");
        }

        // QR Code
        public async Task<IActionResult> QRCode(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentRepository.GetWithDetailsAsync(id);

            if (appointment == null || appointment.UserId != userId)
                return NotFound();

            var queueNumber = await _appointmentRepository
                .GetQueueNumberAsync(appointment.BranchId,
                    appointment.AppointmentDate,
                    appointment.ServiceTypeId,
                    appointment.TimeSlot);

            var qrBase64 = _qrService.GenerateQRCode(appointment, queueNumber);
            ViewBag.QRCode = qrBase64;
            ViewBag.QueueNumber = queueNumber;

            return View(appointment);
        }
    }
}
