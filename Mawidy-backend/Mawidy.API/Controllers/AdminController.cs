using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.DTOs;
using Mawidy.Application.DTOs.Appointments;
using Mawidy.Application.DTOs.Branches;
using Mawidy.Application.DTOs.Common;
using Mawidy.Application.DTOs.Complaints;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBranchRepository _branchRepository;
        private readonly IComplaintRepository _complaintRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly IPdfReportService _pdfReportService;

        public AdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IBranchRepository branchRepository,
            IComplaintRepository complaintRepository,
            IRatingRepository ratingRepository,
            IPdfReportService IPdfReportService)
        {
            _context = context;
            _userManager = userManager;
            _branchRepository = branchRepository;
            _complaintRepository = complaintRepository;
            _ratingRepository = ratingRepository;
            _pdfReportService = IPdfReportService;
        }

        // GET api/admin/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<object>>> GetDashboard()
        {
            var today = DateTime.Today;

            var allAppointments = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Branch)
                    .ThenInclude(b => b.Governorate)
                .ToListAsync();

            var totalAppointments = allAppointments.Count;

            var result = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalBranches = await _context.Branches.CountAsync(),
                TotalAppointments = totalAppointments,
                TotalComplaints = await _context.Complaints.CountAsync(),

                TodayAppointments = allAppointments
                    .Count(a => a.AppointmentDate.Date == today),

                TodayCompleted = allAppointments
                    .Count(a => a.AppointmentDate.Date == today
                        && a.Status == AppointmentStatus.Completed),

                TodayCancelled = allAppointments
                    .Count(a => a.AppointmentDate.Date == today
                        && a.Status == AppointmentStatus.Cancelled),

                PendingComplaints = await _context.Complaints
                    .CountAsync(c => c.Status == ComplaintStatus.Submitted
                        || c.Status == ComplaintStatus.UnderReview),

                CancellationRate = totalAppointments > 0
                    ? Math.Round((double)allAppointments
                        .Count(a => a.Status == AppointmentStatus.Cancelled)
                        / totalAppointments * 100, 1)
                    : 0,

                TopServices = allAppointments
                    .GroupBy(a => a.ServiceType.Name)
                    .Select(g => new
                    {
                        ServiceName = g.Key,
                        TotalAppointments = g.Count(),
                        Percentage = totalAppointments > 0
                            ? Math.Round((double)g.Count() / totalAppointments * 100, 1)
                            : 0
                    })
                    .OrderByDescending(s => s.TotalAppointments)
                    .Take(5)
                    .ToList(),

                GovernorateDistribution = allAppointments
                    .GroupBy(a => a.Branch.Governorate.Name)
                    .Select(g => new
                    {
                        GovernorateName = g.Key,
                        TotalAppointments = g.Count()
                    })
                    .OrderByDescending(g => g.TotalAppointments)
                    .ToList(),

                WeeklyAppointments = allAppointments
                    .Where(a => a.AppointmentDate.Date >= today.AddDays(-7))
                    .GroupBy(a => a.AppointmentDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("dd/MM"),
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return Ok(ApiResponse<object>.Ok(result));
        }

        // GET api/admin/appointments
        [HttpGet("appointments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentDto>>>> GetAppointments(
            DateTime? date, int? branchId)
        {
            var selectedDate = date ?? DateTime.Today;

            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Branch)
                .Include(a => a.ServiceType)
                .Where(a => a.AppointmentDate.Date == selectedDate.Date);

            if (branchId.HasValue)
                query = query.Where(a => a.BranchId == branchId);

            var appointments = await query
                .OrderBy(a => a.TimeSlot)
                .ToListAsync();

            var result = appointments.Select(a => new AppointmentDto
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
                AppointmentDate = a.AppointmentDate,
                TimeSlot = a.TimeSlot,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt
            });

            return Ok(ApiResponse<IEnumerable<AppointmentDto>>.Ok(result));
        }

        // PUT api/admin/appointments/{id}/status
        [HttpPut("appointments/{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateAppointmentStatus(
            int id, [FromBody] UpdateStatusDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ?????"));

            appointment.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ????? ???? ??????"));
        }

        // GET api/admin/complaints
        [HttpGet("complaints")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComplaintDto>>>> GetComplaints(
            int? status)
        {
            var complaints = await _complaintRepository
                .GetAllWithDetailsAsync(status.HasValue ? (ComplaintStatus)status : null);

            var result = complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                UserFullName = c.User.FullName,
                Title = c.Title,
                Description = c.Description,
                Status = c.Status.ToString(),
                AdminResponse = c.AdminResponse,
                AppointmentId = c.AppointmentId,
                CreatedAt = c.CreatedAt
            });

            return Ok(ApiResponse<IEnumerable<ComplaintDto>>.Ok(result));
        }

        // PUT api/admin/complaints/{id}/respond
        [HttpPut("complaints/{id}/respond")]
        public async Task<ActionResult<ApiResponse<string>>> RespondToComplaint(
            int id, RespondComplaintDto dto)
        {
            var complaint = await _complaintRepository.GetByIdAsync(id);
            if (complaint == null)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ??????"));

            complaint.AdminResponse = dto.AdminResponse;
            complaint.Status = dto.Status;

            _complaintRepository.Update(complaint);
            await _complaintRepository.SaveChangesAsync();

            // ???? ????? ???????
            var fullComplaint = await _complaintRepository.GetWithDetailsAsync(id);
            try
            {
                // ??? ????? ??????? ?? ?? ?? ??? ?????
            }
            catch { }

            return Ok(ApiResponse<string>.Ok("", "?? ???? ??? ??????"));
        }

        // GET api/admin/branches
        [HttpGet("branches")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BranchDto>>>> GetBranches()
        {
            var branches = await _branchRepository.GetAllWithDetailsAsync();

            var result = branches.Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                GovernorateId = b.GovernorateId,
                GovernorateName = b.Governorate.Name,
                WorkingDaysCount = b.Schedules.Count
            });

            return Ok(ApiResponse<IEnumerable<BranchDto>>.Ok(result));
        }

        // POST api/admin/branches
        [HttpPost("branches")]
        public async Task<ActionResult<ApiResponse<BranchDto>>> CreateBranch(CreateBranchDto dto)
        {
            var governorate = await _context.Governorates.FindAsync(dto.GovernorateId);
            if (governorate == null)
                return NotFound(ApiResponse<BranchDto>.Fail("???????? ??? ??????"));

            var branch = new Branch
            {
                Name = dto.Name,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                GovernorateId = dto.GovernorateId
            };

            await _branchRepository.AddAsync(branch);
            await _branchRepository.SaveChangesAsync();

            return Ok(ApiResponse<BranchDto>.Ok(new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                Latitude = branch.Latitude,
                Longitude = branch.Longitude,
                GovernorateId = branch.GovernorateId,
                GovernorateName = governorate.Name
            }, "?? ????? ????? ?????"));
        }

        // PUT api/admin/branches/{id}
        [HttpPut("branches/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateBranch(
            int id, CreateBranchDto dto)
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null)
                return NotFound(ApiResponse<string>.Fail("????? ??? ?????"));

            branch.Name = dto.Name;
            branch.Address = dto.Address;
            branch.GovernorateId = dto.GovernorateId;

            // ?????????? ????????
            if (dto.Latitude != 0 && dto.Longitude != 0)
            {
                branch.Latitude = dto.Latitude;
                branch.Longitude = dto.Longitude;
            }

            _branchRepository.Update(branch);
            await _branchRepository.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ????? ????? ?????"));
        }

        // DELETE api/admin/branches/{id}
        [HttpDelete("branches/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteBranch(int id)
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null)
                return NotFound(ApiResponse<string>.Fail("????? ??? ?????"));

            _branchRepository.Delete(branch);
            await _branchRepository.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ??? ????? ?????"));
        }

        // POST api/admin/branches/{id}/schedules
        [HttpPost("branches/{id}/schedules")]
        public async Task<ActionResult<ApiResponse<string>>> AddSchedule(
            int id, AddScheduleDto dto)
        {
            var exists = await _context.BranchSchedules
                .AnyAsync(s => s.BranchId == id && s.DayOfWeek == dto.DayOfWeek);

            if (exists)
                return BadRequest(ApiResponse<string>.Fail("????? ?? ????? ??????"));

            _context.BranchSchedules.Add(new BranchSchedule
            {
                BranchId = id,
                DayOfWeek = dto.DayOfWeek,
                OpenTime = dto.OpenTime,
                CloseTime = dto.CloseTime,
                PeakStartTime = dto.PeakStartTime,
                PeakEndTime = dto.PeakEndTime,
                MaxAppointmentsPerSlot = dto.MaxAppointmentsPerSlot
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("", "?? ????? ??? ?????"));
        }

        // DELETE api/admin/branches/schedules/{id}
        [HttpDelete("branches/schedules/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSchedule(int id)
        {
            var schedule = await _context.BranchSchedules.FindAsync(id);
            if (schedule == null)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ?????"));

            _context.BranchSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ??? ??? ?????"));
        }

        // POST api/admin/branches/{id}/holidays
        [HttpPost("branches/{id}/holidays")]
        public async Task<ActionResult<ApiResponse<string>>> AddHoliday(
            int id, AddHolidayDto dto)
        {
            var exists = await _context.BranchHolidays
                .AnyAsync(h => h.BranchId == id && h.Date.Date == dto.Date.Date);

            if (exists)
                return BadRequest(ApiResponse<string>.Fail("??????? ?? ?????? ??????"));

            _context.BranchHolidays.Add(new BranchHoliday
            {
                BranchId = id,
                Date = dto.Date,
                Reason = dto.Reason
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("", "?? ????? ???????"));
        }

        // DELETE api/admin/branches/holidays/{id}
        [HttpDelete("branches/holidays/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteHoliday(int id)
        {
            var holiday = await _context.BranchHolidays.FindAsync(id);
            if (holiday == null)
                return NotFound(ApiResponse<string>.Fail("??????? ??? ??????"));

            _context.BranchHolidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ??? ???????"));
        }

        // POST api/admin/branches/{id}/service-unavailability
        [HttpPost("branches/{id}/service-unavailability")]
        public async Task<ActionResult<ApiResponse<string>>> AddServiceUnavailability(
            int id, AddServiceUnavailabilityDto dto)
        {
            var exists = await _context.ServiceUnavailabilities
                .AnyAsync(s => s.BranchId == id
                    && s.ServiceTypeId == dto.ServiceTypeId
                    && s.Date.Date == dto.Date.Date);

            if (exists)
                return BadRequest(ApiResponse<string>.Fail("?????? ?? ????? ?????? ?? ????? ??"));

            _context.ServiceUnavailabilities.Add(new ServiceUnavailability
            {
                BranchId = id,
                ServiceTypeId = dto.ServiceTypeId,
                Date = dto.Date,
                Reason = dto.Reason
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("", "?? ????? ??????"));
        }

        // GET api/admin/services
        [HttpGet("services")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetServices()
        {
            var services = await _context.ServiceTypes
                .Include(s => s.Appointments)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DurationMinutes,
                    s.RequiredDocuments,
                    TotalAppointments = s.Appointments.Count
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<object>>.Ok(services));
        }

        // POST api/admin/services
        [HttpPost("services")]
        public async Task<ActionResult<ApiResponse<string>>> CreateService(CreateServiceDto dto)
        {
            _context.ServiceTypes.Add(new ServiceType
            {
                Name = dto.Name,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                RequiredDocuments = dto.RequiredDocuments
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("", "?? ????? ??????"));
        }

        // PUT api/admin/services/{id}
        [HttpPut("services/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateService(
            int id, CreateServiceDto dto)
        {
            var service = await _context.ServiceTypes.FindAsync(id);
            if (service == null)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ??????"));

            service.Name = dto.Name;
            service.Description = dto.Description;
            service.DurationMinutes = dto.DurationMinutes;
            service.RequiredDocuments = dto.RequiredDocuments;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("", "?? ????? ??????"));
        }

        // DELETE api/admin/services/{id}
        [HttpDelete("services/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteService(int id)
        {
            var service = await _context.ServiceTypes.FindAsync(id);
            if (service == null)
                return NotFound(ApiResponse<string>.Fail("?????? ??? ??????"));

            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.ServiceTypeId == id);

            if (hasAppointments)
                return BadRequest(ApiResponse<string>
                    .Fail("?? ???? ???? ?????? ?? ???? ???? ??????"));

            _context.ServiceTypes.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ??? ??????"));
        }

        // GET api/admin/reports/daily
        [HttpGet("reports/daily")]
        public async Task<IActionResult> GetDailyReport(DateTime? date, int? branchId)
        {
            var selectedDate = date ?? DateTime.Today;
            var pdfBytes = await _pdfReportService.GenerateDailyReportAsync(selectedDate, branchId);
            var fileName = $"?????_{selectedDate:yyyy-MM-dd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}


