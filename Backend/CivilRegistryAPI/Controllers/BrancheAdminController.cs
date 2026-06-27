using CivilRegistryAPI.Data;
using CivilRegistryAPI.DTOs;
using CivilRegistryAPI.DTOs.Appointments;
using CivilRegistryAPI.DTOs.Branches;
using CivilRegistryAPI.DTOs.Common;
using CivilRegistryAPI.DTOs.Ratings;
using CivilRegistryAPI.Models;
using CivilRegistryAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CivilRegistryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.BranchAdmin)]
    public class BranchAdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchAdminController(
            AppDbContext context,
            IAppointmentRepository appointmentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _appointmentRepository = appointmentRepository;
            _userManager = userManager;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private async Task<int?> GetBranchIdAsync()
        {
            var user = await _userManager.FindByIdAsync(UserId);
            return user?.BranchId;
        }

        // GET api/branchadmin/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<object>>> GetDashboard()
        {
            var branchId = await GetBranchIdAsync();
            if (branchId == null)
                return BadRequest(ApiResponse<object>.Fail("مش مرتبط بفرع"));

            var today = DateTime.Today;
            var appointments = await _appointmentRepository
                .GetByBranchAndDateAsync(branchId.Value, today);

            var branch = await _context.Branches.FindAsync(branchId.Value);

            var result = new
            {
                BranchName = branch?.Name,
                TodayAppointments = appointments.Count(),
                TodayCompleted = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                TodayCancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                AverageRating = await _context.Ratings
                    .Where(r => r.BranchId == branchId)
                    .AverageAsync(r => (double?)r.Stars) ?? 0,
                TodayAppointmentsList = appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    UserFullName = a.User.FullName,
                    UserNationalId = a.User.NationalId,
                    UserPhone = a.User.PhoneNumber ?? "",
                    ServiceName = a.ServiceType.Name,
                    AppointmentDate = a.AppointmentDate,
                    TimeSlot = a.TimeSlot,
                    Status = a.Status.ToString()
                })
            };

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("governorates")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetGovernorates()
        {
            var governorates = await _context.Governorates
                .OrderBy(g => g.Name)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.CenterLatitude,
                    g.CenterLongitude
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<object>>.Ok(governorates));
        }

        // PUT api/branchadmin/appointments/{id}/complete
        [HttpPut("appointments/{id}/complete")]
        public async Task<ActionResult<ApiResponse<string>>> CompleteAppointment(int id)
        {
            var branchId = await GetBranchIdAsync();
            if (branchId == null)
                return BadRequest(ApiResponse<string>.Fail("مش مرتبط بفرع"));

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.BranchId == branchId);

            if (appointment == null)
                return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

            if (appointment.Status != AppointmentStatus.Confirmed)
                return BadRequest(ApiResponse<string>.Fail("الموعد ده مش مؤكد"));

            appointment.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "تم إكمال الموعد بنجاح"));
        }

        // POST api/branchadmin/holidays
        [HttpPost("holidays")]
        public async Task<ActionResult<ApiResponse<string>>> AddHoliday(AddHolidayDto dto)
        {
            var branchId = await GetBranchIdAsync();
            if (branchId == null)
                return BadRequest(ApiResponse<string>.Fail("مش مرتبط بفرع"));

            var exists = await _context.BranchHolidays
                .AnyAsync(h => h.BranchId == branchId && h.Date.Date == dto.Date.Date);

            if (exists)
                return BadRequest(ApiResponse<string>.Fail("الإجازة دي موجودة بالفعل"));

            _context.BranchHolidays.Add(new BranchHoliday
            {
                BranchId = branchId.Value,
                Date = dto.Date,
                Reason = dto.Reason
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("", "تم إضافة الإجازة"));
        }

        // POST api/branchadmin/service-unavailability
        [HttpPost("service-unavailability")]
        public async Task<ActionResult<ApiResponse<string>>> AddServiceUnavailability(
            AddServiceUnavailabilityDto dto)
        {
            var branchId = await GetBranchIdAsync();
            if (branchId == null)
                return BadRequest(ApiResponse<string>.Fail("مش مرتبط بفرع"));

            var exists = await _context.ServiceUnavailabilities
                .AnyAsync(s => s.BranchId == branchId
                    && s.ServiceTypeId == dto.ServiceTypeId
                    && s.Date.Date == dto.Date.Date);

            if (exists)
                return BadRequest(ApiResponse<string>
                    .Fail("الخدمة دي معطلة بالفعل في اليوم ده"));

            _context.ServiceUnavailabilities.Add(new ServiceUnavailability
            {
                BranchId = branchId.Value,
                ServiceTypeId = dto.ServiceTypeId,
                Date = dto.Date,
                Reason = dto.Reason
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("", "تم تعطيل الخدمة"));
        }
    }
}
