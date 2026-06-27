using CivilRegistryAPI.DTOs.Common;
using CivilRegistryAPI.DTOs.Ratings;
using CivilRegistryAPI.Models;
using CivilRegistryAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CivilRegistryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public RatingsController(
            IRatingRepository ratingRepository,
            IAppointmentRepository appointmentRepository)
        {
            _ratingRepository = ratingRepository;
            _appointmentRepository = appointmentRepository;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET api/ratings/recent
        [AllowAnonymous]
        [HttpGet("recent")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RatingDto>>>> GetRecentRatings()
        {
            var ratings = await _ratingRepository.GetRecentPositiveRatingsAsync(4);

            var result = ratings.Select(r => new RatingDto
            {
                Id = r.Id,
                UserFullName = MaskName(r.User.FullName),
                Stars = r.Stars,
                Comment = r.Comment,
                ServiceName = r.Appointment.ServiceType.Name,
                BranchName = r.Branch.Name,
                CreatedAt = r.CreatedAt
            });

            return Ok(ApiResponse<IEnumerable<RatingDto>>.Ok(result));
        }

        // POST api/ratings
        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> CreateRating(CreateRatingDto dto)
        {
            var appointment = await _appointmentRepository.GetWithDetailsAsync(dto.AppointmentId);

            if (appointment == null || appointment.UserId != UserId)
                return NotFound(ApiResponse<string>.Fail("الموعد غير موجود"));

            if (appointment.Status != AppointmentStatus.Completed)
                return BadRequest(ApiResponse<string>.Fail("الموعد ده مش مكتمل"));

            var alreadyRated = await _ratingRepository.HasRatedAsync(dto.AppointmentId);
            if (alreadyRated)
                return BadRequest(ApiResponse<string>.Fail("قيمت الموعد ده قبل كده"));

            var rating = new Rating
            {
                UserId = UserId,
                AppointmentId = dto.AppointmentId,
                BranchId = appointment.BranchId,
                Stars = dto.Stars,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };

            await _ratingRepository.AddAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "شكرا على تقييمك!"));
        }

        private string MaskName(string fullName)
        {
            var parts = fullName.Split(' ');
            return parts.Length >= 2
                ? $"{parts[0]} {parts[1][0]}."
                : parts[0];
        }
    }
}
