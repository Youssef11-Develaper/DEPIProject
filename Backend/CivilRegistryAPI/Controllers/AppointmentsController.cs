using CivilRegistryAPI.Data;
using CivilRegistryAPI.DTOs.Appointments;
using CivilRegistryAPI.DTOs.Common;
using CivilRegistryAPI.Models;
using CivilRegistryAPI.Repositories.Interfaces;
using CivilRegistryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CivilRegistryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly QRService _qrService;
        private readonly AppointmentAvailabilityService _availabilityService;

        public AppointmentsController(
            IAppointmentRepository appointmentRepository,
            IBranchRepository branchRepository,
            AppDbContext context,
            EmailService emailService,
            QRService qrService,
            AppointmentAvailabilityService availabilityService)
        {
            _appointmentRepository = appointmentRepository;
            _branchRepository = branchRepository;
            _context = context;
            _emailService = emailService;
            _qrService = qrService;
            _availabilityService = availabilityService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET api/appointments
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentDto>>>> GetMyAppointments()
        {
            var appointments = await _appointmentRepository.GetUserAppointmentsAsync(UserId);

            var result = await Task.WhenAll(appointments.Select(async a => new AppointmentDto
            {
                Id = a.Id,
                UserFullName = a.User.FullName,
                UserNationalId = a.User.NationalId,
                UserPhone = a.User.PhoneNumber ?? "",
                BranchId = a.BranchId,
                BranchName = a.Branch.Name,
                BranchAddress = a.Branch.Address,
                ServiceTypeId = a.ServiceTypeId,
                ServiceName = a.ServiceType.Name,
                RequiredDocuments = a.ServiceType.RequiredDocuments,
                AppointmentDate = a.AppointmentDate,
                TimeSlot = a.TimeSlot,
                Status = a.Status.ToString(),
                QueueNumber = await _appointmentRepository.GetQueueNumberAsync(
                    a.BranchId, a.AppointmentDate, a.ServiceTypeId, a.TimeSlot),
                IsNotified = a.IsNotified,
                CreatedAt = a.CreatedAt,
                HasRating = a.Rating != null
            }));

            return Ok(ApiResponse<IEnumerable<AppointmentDto>>.Ok(result));
        }

        // GET api/appointments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetWithDetailsAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<AppointmentDto>.Fail("الموعد غير موجود"));

            var queueNumber = await _appointmentRepository.GetQueueNumberAsync(
                appointment.BranchId, appointment.AppointmentDate,
                appointment.ServiceTypeId, appointment.TimeSlot);

            return Ok(ApiResponse<AppointmentDto>.Ok(new AppointmentDto
            {
                Id = appointment.Id,
                UserFullName = appointment.User.FullName,
                UserNationalId = appointment.User.NationalId,
                UserPhone = appointment.User.PhoneNumber ?? "",
                BranchId = appointment.BranchId,
                BranchName = appointment.Branch.Name,
                BranchAddress = appointment.Branch.Address,
                ServiceTypeId = appointment.ServiceTypeId,
                ServiceName = appointment.ServiceType.Name,
                RequiredDocuments = appointment.ServiceType.RequiredDocuments,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = appointment.TimeSlot,
                Status = appointment.Status.ToString(),
                QueueNumber = queueNumber,
                IsNotified = appointment.IsNotified,
                CreatedAt = appointment.CreatedAt,
                HasRating = appointment.Rating != null
            }));
        }

        // GET api/appointments/slots?branchId=1&serviceTypeId=1&date=2024-01-01
        [HttpGet("slots")]
        public async Task<ActionResult<ApiResponse<AvailableSlotsDto>>> GetAvailableSlots(
            int branchId, int serviceTypeId, DateTime date)
        {
            var viewModel = await _availabilityService
                .BuildSlotViewModelAsync(branchId, serviceTypeId, date);

            var result = new AvailableSlotsDto
            {
                BranchId = viewModel.BranchId,
                BranchName = viewModel.BranchName,
                ServiceTypeId = viewModel.ServiceTypeId,
                ServiceName = viewModel.ServiceName,
                SelectedDate = viewModel.SelectedDate,
                IsAvailable = viewModel.IsAvailable,
                UnavailabilityMessage = viewModel.UnavailabilityMessage,
                BusiestDayName = viewModel.BusiestDayName,
                BusiestPeriod = viewModel.BusiestPeriod,
                Slots = viewModel.Slots.Select(s => new SlotDto
                {
                    Time = s.Time,
                    IsBooked = s.IsBooked,
                    IsPeak = s.IsPeak,
                    BookedCount = s.BookedCount,
                    MaxCount = s.MaxCount
                }).ToList()
            };

            return Ok(ApiResponse<AvailableSlotsDto>.Ok(result));
        }

        // POST api/appointments
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> CreateAppointment(
            CreateAppointmentDto dto)
        {
            // نتأكد مش حجز نفس الخدمة في نفس اليوم
            var hasAppointment = await _appointmentRepository
                .HasAppointmentSameDayAsync(UserId, dto.AppointmentDate, dto.ServiceTypeId);

            if (hasAppointment)
                return BadRequest(ApiResponse<AppointmentDto>
                    .Fail("حجزت الخدمة دي قبل كده في نفس اليوم"));

            // نتأكد الـ slot مش كامل
            var bookedCount = await _appointmentRepository
                .GetBookedCountAsync(dto.BranchId, dto.AppointmentDate, dto.TimeSlot);

            var branch = await _branchRepository.GetWithDetailsAsync(dto.BranchId);
            if (branch == null)
                return NotFound(ApiResponse<AppointmentDto>.Fail("الفرع غير موجود"));

            var schedule = branch.Schedules
                .FirstOrDefault(s => s.DayOfWeek == dto.AppointmentDate.DayOfWeek);

            if (schedule == null)
                return BadRequest(ApiResponse<AppointmentDto>
                    .Fail("الفرع مش بيشتغل في اليوم ده"));

            if (bookedCount >= schedule.MaxAppointmentsPerSlot)
                return BadRequest(ApiResponse<AppointmentDto>
                    .Fail("الموعد ده مكتمل، اختار موعد تاني"));

            var appointment = new Appointment
            {
                UserId = UserId,
                BranchId = dto.BranchId,
                ServiceTypeId = dto.ServiceTypeId,
                AppointmentDate = dto.AppointmentDate,
                TimeSlot = dto.TimeSlot,
                Status = AppointmentStatus.Confirmed,
                CreatedAt = DateTime.Now
            };

            await _appointmentRepository.AddAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            // جيب التفاصيل الكاملة
            var fullAppointment = await _appointmentRepository.GetWithDetailsAsync(appointment.Id);

            // بعت إيميل تأكيد
            try
            {
                var user = fullAppointment!.User;
                await _emailService.SendAppointmentConfirmationAsync(user, fullAppointment);
            }
            catch { }

            var queueNumber = await _appointmentRepository.GetQueueNumberAsync(
                appointment.BranchId, appointment.AppointmentDate,
                appointment.ServiceTypeId, appointment.TimeSlot);

            return Ok(ApiResponse<AppointmentDto>.Ok(new AppointmentDto
            {
                Id = fullAppointment!.Id,
                UserFullName = fullAppointment.User.FullName,
                UserNationalId = fullAppointment.User.NationalId,
                UserPhone = fullAppointment.User.PhoneNumber ?? "",
                BranchId = fullAppointment.BranchId,
                BranchName = fullAppointment.Branch.Name,
                BranchAddress = fullAppointment.Branch.Address,
                ServiceTypeId = fullAppointment.ServiceTypeId,
                ServiceName = fullAppointment.ServiceType.Name,
                RequiredDocuments = fullAppointment.ServiceType.RequiredDocuments,
                AppointmentDate = fullAppointment.AppointmentDate,
                TimeSlot = fullAppointment.TimeSlot,
                Status = fullAppointment.Status.ToString(),
                QueueNumber = queueNumber,
                IsNotified = fullAppointment.IsNotified,
                CreatedAt = fullAppointment.CreatedAt,
                HasRating = false
            }, "تم الحجز بنجاح"));
        }

        // PUT api/appointments/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<string>>> CancelAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled)
                return BadRequest(ApiResponse<string>.Fail("مش تقدر تلغي الموعد ده"));

            appointment.Status = AppointmentStatus.Cancelled;
            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "تم إلغاء الموعد بنجاح"));
        }

        // PUT api/appointments/{id}/reschedule
        [HttpPut("{id}/reschedule")]
        public async Task<ActionResult<ApiResponse<string>>> RescheduleAppointment(
            int id, RescheduleAppointmentDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

            if (appointment.Status != AppointmentStatus.Confirmed &&
                appointment.Status != AppointmentStatus.Pending)
                return BadRequest(ApiResponse<string>.Fail("مش تقدر تأجل الموعد ده"));

            var bookedCount = await _appointmentRepository
                .GetBookedCountAsync(appointment.BranchId, dto.NewDate, dto.NewTimeSlot);

            var branch = await _branchRepository.GetWithDetailsAsync(appointment.BranchId);
            var schedule = branch?.Schedules
                .FirstOrDefault(s => s.DayOfWeek == dto.NewDate.DayOfWeek);

            if (schedule == null || bookedCount >= schedule.MaxAppointmentsPerSlot)
                return BadRequest(ApiResponse<string>.Fail("الموعد ده مش متاح، اختار موعد تاني"));

            appointment.AppointmentDate = dto.NewDate;
            appointment.TimeSlot = dto.NewTimeSlot;
            appointment.Status = AppointmentStatus.Confirmed;
            appointment.IsNotified = false;

            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "تم تأجيل الموعد بنجاح"));
        }

        // GET api/appointments/{id}/qr
        [HttpGet("{id}/qr")]
        public async Task<ActionResult<ApiResponse<string>>> GetQRCode(int id)
        {
            var appointment = await _appointmentRepository.GetWithDetailsAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

            var queueNumber = await _appointmentRepository.GetQueueNumberAsync(
                appointment.BranchId, appointment.AppointmentDate,
                appointment.ServiceTypeId, appointment.TimeSlot);

            var qrBase64 = _qrService.GenerateQRCode(appointment, queueNumber);

            return Ok(ApiResponse<string>.Ok(qrBase64, ""));
        }
    }
}
