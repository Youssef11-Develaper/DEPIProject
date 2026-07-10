using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.DTOs.Common;
using Mawidy.Application.DTOs.Complaints;
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
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IEmailService _emailService;

        public ComplaintsController(
            IComplaintRepository complaintRepository,
            IAppointmentRepository appointmentRepository,
            IEmailService IEmailService)
        {
            _complaintRepository = complaintRepository;
            _appointmentRepository = appointmentRepository;
            _emailService = IEmailService;
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
                return NotFound(ApiResponse<ComplaintDto>.Fail("?????? ??? ??????"));

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
            // التحقق من صحة الموعد إذا تم اختياره
            if (dto.AppointmentId.HasValue)
            {
                var appointment = await _appointmentRepository
                    .GetWithDetailsAsync(dto.AppointmentId.Value);

                if (appointment == null || appointment.UserId != UserId)
                    return BadRequest(ApiResponse<ComplaintDto>
                        .Fail("الموعد المحدد غير صحيح أو لا يخصك"));

                if (appointment.Status != AppointmentStatus.Completed)
                    return BadRequest(ApiResponse<ComplaintDto>
                        .Fail("الموعد المحدد يجب أن يكون مكتملاً لتقديم شكوى بشأنه"));
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
            }, "?? ????? ????? ?????"));
        }
    }
}


