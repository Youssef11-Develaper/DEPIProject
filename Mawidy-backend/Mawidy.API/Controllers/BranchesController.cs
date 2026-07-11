using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.DTOs.Branches;
using Mawidy.Application.DTOs.Common;
using Mawidy.Application.DTOs.Ratings;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly AppDbContext _context;

        public BranchesController(
            IBranchRepository branchRepository,
            IRatingRepository ratingRepository,
            AppDbContext context)
        {
            _branchRepository = branchRepository;
            _ratingRepository = ratingRepository;
            _context = context;
        }

        // GET api/branches
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BranchDto>>>> GetAll()
        {
            var branches = await _branchRepository.GetAllWithDetailsAsync();
            var result = await MapBranchesAsync(branches);
            return Ok(ApiResponse<IEnumerable<BranchDto>>.Ok(result));
        }

        // GET api/branches/by-governorate/{governorateId}
        [HttpGet("by-governorate/{governorateId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BranchDto>>>> GetByGovernorate(
            int governorateId)
        {
            var branches = await _branchRepository.GetByGovernorateAsync(governorateId);
            var result = await MapBranchesAsync(branches);
            return Ok(ApiResponse<IEnumerable<BranchDto>>.Ok(result));
        }

        // GET api/branches/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BranchDto>>> GetById(int id)
        {
            var branch = await _branchRepository.GetWithDetailsAsync(id);
            if (branch == null)
                return NotFound(ApiResponse<BranchDto>.Fail("الفرع غير موجود"));

            var averageRating = await _ratingRepository.GetBranchAverageAsync(id);

            var result = new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                Latitude = branch.Latitude,
                Longitude = branch.Longitude,
                GovernorateId = branch.GovernorateId,
                GovernorateName = branch.Governorate.Name,
                WorkingDaysCount = branch.Schedules.Count,
                AverageRating = averageRating,
                OperatorId = branch.OperatorId,
                DistrictId = branch.DistrictId,
                Schedules = branch.Schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    DayName = GetDayName(s.DayOfWeek),
                    OpenTime = s.OpenTime,
                    CloseTime = s.CloseTime,
                    PeakStartTime = s.PeakStartTime,
                    PeakEndTime = s.PeakEndTime,
                    MaxAppointmentsPerSlot = s.MaxAppointmentsPerSlot
                }).ToList(),
                Holidays = branch.Holidays.Select(h => new HolidayDto
                {
                    Id = h.Id,
                    Date = h.Date,
                    Reason = h.Reason
                }).ToList()
            };

            return Ok(ApiResponse<BranchDto>.Ok(result));
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

        // GET api/branches/{id}/ratings
        [HttpGet("{id}/ratings")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RatingDto>>>> GetBranchRatings(int id)
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null)
                return NotFound(ApiResponse<IEnumerable<RatingDto>>.Fail("????? ??? ?????"));

            var ratings = await _ratingRepository.GetBranchRatingsAsync(id);

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

        // GET api/branches/services
        [HttpGet("services")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetServices()
        {
            var services = await _context.ServiceTypes
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DurationMinutes,
                    s.RequiredDocuments
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<object>>.Ok(services));
        }

        // GET api/branches/operators
        [HttpGet("operators")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetOperators()
        {
            var operators = await _context.Operators
                .Select(o => new
                {
                    o.Id,
                    o.Key,
                    o.NameAr,
                    o.Color,
                    o.BgColor,
                    o.Emoji,
                    o.Hotline
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<object>>.Ok(operators));
        }

        // GET api/branches/operator-services
        [HttpGet("operator-services")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetOperatorServices(int? operatorId)
        {
            var query = _context.OperatorServices.AsQueryable();
            if (operatorId.HasValue)
            {
                query = query.Where(s => s.OperatorId == operatorId.Value);
            }

            var services = await query
                .Select(s => new
                {
                    s.Id,
                    s.OperatorId,
                    s.ServiceKey,
                    s.Icon,
                    s.NameAr,
                    s.EstimatedTime
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<object>>.Ok(services));
        }

        // GET api/branches/service-documents/{serviceKey}
        [HttpGet("service-documents/{serviceKey}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetServiceDocuments(string serviceKey)
        {
            var docs = await _context.ServiceDocuments
                .Where(d => d.ServiceKey == serviceKey)
                .OrderBy(d => d.SortOrder)
                .Select(d => new
                {
                    d.Id,
                    d.ServiceKey,
                    d.DocType,
                    d.TextAr,
                    d.NoteAr,
                    d.SortOrder
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<object>>.Ok(docs));
        }

        // Helper Methods
        private async Task<IEnumerable<BranchDto>> MapBranchesAsync(IEnumerable<Branch> branches)
        {
            var result = new List<BranchDto>();

            foreach (var branch in branches)
            {
                var averageRating = await _ratingRepository.GetBranchAverageAsync(branch.Id);
                result.Add(new BranchDto
                {
                    Id = branch.Id,
                    Name = branch.Name,
                    Address = branch.Address,
                    Latitude = branch.Latitude,
                    Longitude = branch.Longitude,
                    GovernorateId = branch.GovernorateId,
                    GovernorateName = branch.Governorate.Name,
                    WorkingDaysCount = branch.Schedules.Count,
                    AverageRating = averageRating,
                    OperatorId = branch.OperatorId,
                    DistrictId = branch.DistrictId,
                    Schedules = branch.Schedules.Select(s => new ScheduleDto
                    {
                        Id = s.Id,
                        DayName = GetDayName(s.DayOfWeek),
                        OpenTime = s.OpenTime,
                        CloseTime = s.CloseTime,
                        PeakStartTime = s.PeakStartTime,
                        PeakEndTime = s.PeakEndTime,
                        MaxAppointmentsPerSlot = s.MaxAppointmentsPerSlot
                    }).ToList()
                });
            }

            return result;
        }

        private string GetDayName(DayOfWeek day) => day switch
        {
            DayOfWeek.Sunday => "?????",
            DayOfWeek.Monday => "???????",
            DayOfWeek.Tuesday => "????????",
            DayOfWeek.Wednesday => "????????",
            DayOfWeek.Thursday => "??????",
            DayOfWeek.Friday => "??????",
            DayOfWeek.Saturday => "?????",
            _ => ""
        };

        private string MaskName(string fullName)
        {
            var parts = fullName.Split(' ');
            return parts.Length >= 2
                ? $"{parts[0]} {parts[1][0]}."
                : parts[0];
        }
    }
}


