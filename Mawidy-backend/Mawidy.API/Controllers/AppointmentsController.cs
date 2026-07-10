using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.DTOs.Appointments;
using Mawidy.Application.DTOs.Common;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mawidy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IQRService _qrService;
        private readonly IAppointmentAvailabilityService _availabilityService;

        public AppointmentsController(
            IAppointmentRepository appointmentRepository,
            IBranchRepository branchRepository,
            AppDbContext context,
            IEmailService IEmailService,
            IQRService IQRService,
            IAppointmentAvailabilityService availabilityService)
        {
            _appointmentRepository = appointmentRepository;
            _branchRepository = branchRepository;
            _context = context;
            _emailService = IEmailService;
            _qrService = IQRService;
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
                return NotFound(ApiResponse<AppointmentDto>.Fail("?????? ??? ?????"));

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
            // ????? ?? ??? ??? ?????? ?? ??? ?????
            var hasAppointment = await _appointmentRepository
                .HasAppointmentSameDayAsync(UserId, dto.AppointmentDate, dto.ServiceTypeId);

            if (hasAppointment)
                return BadRequest(ApiResponse<AppointmentDto>
                    .Fail("???? ?????? ?? ??? ??? ?? ??? ?????"));

            // ????? ??? slot ?? ????
            var bookedCount = await _appointmentRepository
                .GetBookedCountAsync(dto.BranchId, dto.AppointmentDate, dto.TimeSlot);

            var branch = await _branchRepository.GetWithDetailsAsync(dto.BranchId);
            if (branch == null)
                return NotFound(ApiResponse<AppointmentDto>.Fail("????? ??? ?????"));

            var schedule = branch.Schedules
                .FirstOrDefault(s => s.DayOfWeek == dto.AppointmentDate.DayOfWeek);

            if (schedule == null)
                return BadRequest(ApiResponse<AppointmentDto>
                    .Fail("????? ?? ?????? ?? ????? ??"));

            if (bookedCount >= schedule.MaxAppointmentsPerSlot)
                return BadRequest(ApiResponse<AppointmentDto>
                    .Fail("?????? ?? ?????? ????? ???? ????"));

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

            // ??? ???????? ???????
            var fullAppointment = await _appointmentRepository.GetWithDetailsAsync(appointment.Id);

            // ??? ????? ?????
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
            }, "?? ????? ?????"));
        }

        // PUT api/appointments/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<string>>> CancelAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ?????"));

            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled)
                return BadRequest(ApiResponse<string>.Fail("?? ???? ???? ?????? ??"));

            appointment.Status = AppointmentStatus.Cancelled;
            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ????? ?????? ?????"));
        }

        // PUT api/appointments/{id}/reschedule
        [HttpPut("{id}/reschedule")]
        public async Task<ActionResult<ApiResponse<string>>> RescheduleAppointment(
            int id, RescheduleAppointmentDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ?????"));

            if (appointment.Status != AppointmentStatus.Confirmed &&
                appointment.Status != AppointmentStatus.Pending)
                return BadRequest(ApiResponse<string>.Fail("?? ???? ???? ?????? ??"));

            var bookedCount = await _appointmentRepository
                .GetBookedCountAsync(appointment.BranchId, dto.NewDate, dto.NewTimeSlot);

            var branch = await _branchRepository.GetWithDetailsAsync(appointment.BranchId);
            var schedule = branch?.Schedules
                .FirstOrDefault(s => s.DayOfWeek == dto.NewDate.DayOfWeek);

            if (schedule == null || bookedCount >= schedule.MaxAppointmentsPerSlot)
                return BadRequest(ApiResponse<string>.Fail("?????? ?? ?? ????? ????? ???? ????"));

            appointment.AppointmentDate = dto.NewDate;
            appointment.TimeSlot = dto.NewTimeSlot;
            appointment.Status = AppointmentStatus.Confirmed;
            appointment.IsNotified = false;

            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ????? ?????? ?????"));
        }

        // GET api/appointments/{id}/qr
        [HttpGet("{id}/qr")]
        public async Task<ActionResult<ApiResponse<string>>> GetQRCode(int id)
        {
            var appointment = await _appointmentRepository.GetWithDetailsAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ?????"));

            var queueNumber = await _appointmentRepository.GetQueueNumberAsync(
                appointment.BranchId, appointment.AppointmentDate,
                appointment.ServiceTypeId, appointment.TimeSlot);

            var qrBase64 = _qrService.GenerateQRCode(appointment, queueNumber);

            return Ok(ApiResponse<string>.Ok(qrBase64, ""));
        }
    }
}


