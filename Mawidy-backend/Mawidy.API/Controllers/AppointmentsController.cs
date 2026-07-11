using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.DTOs.Appointments;
using Mawidy.Application.DTOs.Common;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var result = new List<AppointmentDto>();
            foreach (var a in appointments)
            {
                var appointmentDate = a.AppointmentDate;
                var timeSlot = a.TimeSlot;
                if (a.SystemType == SystemType.Bank)
                {
                    if (DateTime.TryParse(a.Date, out var parsedD)) appointmentDate = parsedD;
                    if (TimeSpan.TryParse(a.Time, out var parsedT)) timeSlot = parsedT;
                }

                var queueNumber = await _appointmentRepository.GetQueueNumberAsync(
                    a.BranchId, appointmentDate, a.ServiceTypeId ?? 0, timeSlot);

                result.Add(new AppointmentDto
                {
                    Id = a.Id,
                    StringId = a.Id.ToString(),
                    SystemType = (int)a.SystemType,
                    UserFullName = a.User?.FullName ?? a.CustomerName,
                    UserNationalId = a.User?.NationalId ?? "",
                    UserPhone = a.User?.PhoneNumber ?? a.CustomerPhone,
                    BranchId = a.BranchId,
                    BranchName = a.Branch.Name,
                    BranchAddress = a.Branch.Address,
                    ServiceTypeId = a.ServiceTypeId ?? 0,
                    ServiceName = a.ServiceType?.Name ?? a.Service ?? a.ServiceKey,
                    RequiredDocuments = a.ServiceType?.RequiredDocuments ?? "",
                    AppointmentDate = appointmentDate,
                    TimeSlot = timeSlot,
                    Status = a.Status.ToString(),
                    QueueNumber = queueNumber,
                    IsNotified = a.IsNotified,
                    CreatedAt = a.CreatedAt,
                    HasRating = a.Rating != null
                });
            }

            var user = await _context.Users.FindAsync(UserId);
            var combinedResult = result.ToList();
            if (user != null)
            {
                // 1. Hospital Bed Reservations – match by UserId (if linked) OR by phone (fallback for anonymous bookings)
                var reservations = await _context.HospitalReservations
                    .Include(r => r.Hospitals)
                    .Include(r => r.BedTypes)
                    .Where(r => r.UserId == UserId ||
                                (!string.IsNullOrEmpty(user.PhoneNumber) && r.PatientPhone == user.PhoneNumber))
                    .ToListAsync();

                if (reservations.Any())
                {
                    var resList = reservations.Select(r => new AppointmentDto
                    {
                        Id = r.ReservationId,
                        StringId = r.ReservationId.ToString(),
                        SystemType = (int)Mawidy.Domain.Enums.SystemType.Hospital,
                        UserFullName = r.PatientName,
                        UserNationalId = user.NationalId,
                        UserPhone = r.PatientPhone,
                        BranchId = 0,
                        BranchName = r.Hospitals?.Name ?? "المستشفى",
                        BranchAddress = r.Hospitals?.Address ?? "",
                        ServiceTypeId = 0,
                        ServiceName = $"حجز سرير ({r.BedTypes?.Name ?? "رعاية"})",
                        RequiredDocuments = "",
                        AppointmentDate = r.ExpiresAt,
                        TimeSlot = r.ExpiresAt.TimeOfDay,
                        Status = r.Status == "Accepted" ? "Confirmed" : r.Status == "Rejected" ? "Cancelled" : "Pending",
                        QueueNumber = r.BedId ?? 0,
                        IsNotified = false,
                        CreatedAt = r.ReservedAt,
                        HasRating = false
                    }).ToList();

                    combinedResult = combinedResult.Concat(resList).ToList();
                }

                // 2. Court Bookings
                Guid? userGuid = Guid.TryParse(UserId, out var parsedGuid) ? parsedGuid : null;
                var courtBookingsQuery = _context.Bookings
                    .Include(b => b.Court)
                    .Include(b => b.Department)
                    .Include(b => b.Service)
                    .AsQueryable();

                if (userGuid.HasValue)
                {
                    courtBookingsQuery = courtBookingsQuery.Where(b => b.UserId == userGuid.Value || b.NationalId == user.NationalId || b.PhoneNumber == user.PhoneNumber);
                }
                else
                {
                    courtBookingsQuery = courtBookingsQuery.Where(b => b.NationalId == user.NationalId || b.PhoneNumber == user.PhoneNumber);
                }

                var courtBookings = await courtBookingsQuery.ToListAsync();
                var courtList = courtBookings.Select(b => new AppointmentDto
                {
                    Id = Math.Abs(b.Id.GetHashCode()),
                    StringId = b.Id.ToString(),
                    SystemType = (int)Mawidy.Domain.Enums.SystemType.Court,
                    UserFullName = b.FullName,
                    UserNationalId = b.NationalId,
                    UserPhone = b.PhoneNumber,
                    BranchId = 0,
                    BranchName = b.Court?.Name ?? "المحكمة",
                    BranchAddress = b.Court?.Address ?? "",
                    ServiceTypeId = 0,
                    ServiceName = b.Service?.Name ?? "جلسة قضائية",
                    RequiredDocuments = b.Service?.RequiredDocumentsJson ?? "",
                    AppointmentDate = b.BookingDate,
                    TimeSlot = b.TimeSlot,
                    Status = b.Status.ToString(),
                    QueueNumber = b.QueueNumber,
                    IsNotified = false,
                    CreatedAt = b.CreatedAt,
                    HasRating = false
                }).ToList();

                combinedResult = combinedResult.Concat(courtList).ToList();
            }

            combinedResult = combinedResult.OrderByDescending(x => x.CreatedAt).ToList();
            return Ok(ApiResponse<IEnumerable<AppointmentDto>>.Ok(combinedResult));
        }

        // GET api/appointments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetWithDetailsAsync(id);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<AppointmentDto>.Fail("?????? ??? ?????"));

            var appointmentDate = appointment.AppointmentDate;
            var timeSlot = appointment.TimeSlot;
            if (appointment.SystemType == SystemType.Bank)
            {
                if (DateTime.TryParse(appointment.Date, out var parsedD)) appointmentDate = parsedD;
                if (TimeSpan.TryParse(appointment.Time, out var parsedT)) timeSlot = parsedT;
            }

            var queueNumber = await _appointmentRepository.GetQueueNumberAsync(
                appointment.BranchId, appointmentDate,
                appointment.ServiceTypeId ?? 0, timeSlot);

            return Ok(ApiResponse<AppointmentDto>.Ok(new AppointmentDto
            {
                Id = appointment.Id,
                StringId = appointment.Id.ToString(),
                SystemType = (int)appointment.SystemType,
                UserFullName = appointment.User?.FullName ?? appointment.CustomerName,
                UserNationalId = appointment.User?.NationalId ?? "",
                UserPhone = appointment.User?.PhoneNumber ?? appointment.CustomerPhone,
                BranchId = appointment.BranchId,
                BranchName = appointment.Branch.Name,
                BranchAddress = appointment.Branch.Address,
                ServiceTypeId = appointment.ServiceTypeId ?? 0,
                ServiceName = appointment.ServiceType?.Name ?? appointment.Service ?? appointment.ServiceKey,
                RequiredDocuments = appointment.ServiceType?.RequiredDocuments ?? "",
                AppointmentDate = appointmentDate,
                TimeSlot = timeSlot,
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
                appointment.ServiceTypeId ?? 0, appointment.TimeSlot);

            return Ok(ApiResponse<AppointmentDto>.Ok(new AppointmentDto
            {
                Id = fullAppointment!.Id,
                SystemType = (int)fullAppointment.SystemType,
                UserFullName = fullAppointment.User?.FullName ?? fullAppointment.CustomerName,
                UserNationalId = fullAppointment.User?.NationalId ?? "",
                UserPhone = fullAppointment.User?.PhoneNumber ?? fullAppointment.CustomerPhone,
                BranchId = fullAppointment.BranchId,
                BranchName = fullAppointment.Branch.Name,
                BranchAddress = fullAppointment.Branch.Address,
                ServiceTypeId = fullAppointment.ServiceTypeId ?? 0,
                ServiceName = fullAppointment.ServiceType?.Name ?? fullAppointment.Service ?? fullAppointment.ServiceKey,
                RequiredDocuments = fullAppointment.ServiceType?.RequiredDocuments ?? "",
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

            if (appointment == null || (appointment.UserId != null && appointment.UserId != UserId))
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
                appointment.ServiceTypeId ?? 0, appointment.TimeSlot);

            var qrBase64 = _qrService.GenerateQRCode(appointment, queueNumber);

            return Ok(ApiResponse<string>.Ok(qrBase64, ""));
        }
    }
}


