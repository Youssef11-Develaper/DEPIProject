using CivilRegistryAPI.DTOs.Common;
using CivilRegistryAPI.DTOs.Complaints;
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
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly EmailService _emailService;

        public ComplaintsController(
            IComplaintRepository complaintRepository,
            IAppointmentRepository appointmentRepository,
            EmailService emailService)
        {
            _complaintRepository = complaintRepository;
            _appointmentRepository = appointmentRepository;
            _emailService = emailService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET api/complaints
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComplaintDto>>>> GetMyComplaints()
        {
            var complaints = await _complaintRepository.GetUserComplaintsAsync(UserId);

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

        // GET api/complaints/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ComplaintDto>>> GetComplaint(int id)
        {
            var complaint = await _complaintRepository.GetWithDetailsAsync(id);

            if (complaint == null || complaint.UserId != UserId)
                return NotFound(ApiResponse<ComplaintDto>.Fail("الشكوى غير موجودة"));

            return Ok(ApiResponse<ComplaintDto>.Ok(new ComplaintDto
            {
                Id = complaint.Id,
                UserFullName = complaint.User.FullName,
                Title = complaint.Title,
                Description = complaint.Description,
                Status = complaint.Status.ToString(),
                AdminResponse = complaint.AdminResponse,
                AppointmentId = complaint.AppointmentId,
                CreatedAt = complaint.CreatedAt
            }));
        }

        // POST api/complaints
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ComplaintDto>>> CreateComplaint(
            CreateComplaintDto dto)
        {
            // نتأكد إن عنده موعد مكتمل
            var appointments = await _appointmentRepository.GetUserAppointmentsAsync(UserId);
            var hasCompleted = appointments.Any(a => a.Status == AppointmentStatus.Completed);

            if (!hasCompleted)
                return BadRequest(ApiResponse<ComplaintDto>
                    .Fail("لازم يكون عندك موعد مكتمل عشان تقدر تقدم شكوى"));

            // لو مرتبطة بموعد نتأكد إنه بتاعه
            if (dto.AppointmentId.HasValue)
            {
                var appointment = await _appointmentRepository
                    .GetWithDetailsAsync(dto.AppointmentId.Value);

                if (appointment == null || appointment.UserId != UserId)
                    return BadRequest(ApiResponse<ComplaintDto>
                        .Fail("الموعد ده مش بتاعك"));

                if (appointment.Status != AppointmentStatus.Completed)
                    return BadRequest(ApiResponse<ComplaintDto>
                        .Fail("الموعد ده مش مكتمل"));
            }

            var complaint = new Complaint
            {
                UserId = UserId,
                AppointmentId = dto.AppointmentId,
                Title = dto.Title,
                Description = dto.Description,
                Status = ComplaintStatus.Submitted,
                CreatedAt = DateTime.Now
            };

            await _complaintRepository.AddAsync(complaint);
            await _complaintRepository.SaveChangesAsync();

            var fullComplaint = await _complaintRepository.GetWithDetailsAsync(complaint.Id);

            return Ok(ApiResponse<ComplaintDto>.Ok(new ComplaintDto
            {
                Id = fullComplaint!.Id,
                UserFullName = fullComplaint.User.FullName,
                Title = fullComplaint.Title,
                Description = fullComplaint.Description,
                Status = fullComplaint.Status.ToString(),
                AdminResponse = fullComplaint.AdminResponse,
                AppointmentId = fullComplaint.AppointmentId,
                CreatedAt = fullComplaint.CreatedAt
            }, "تم إرسال شكواك بنجاح"));
        }
    }
}
