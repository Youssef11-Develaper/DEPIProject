using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.DTOs;
using Mawidy.Application.DTOs.Common;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mawidy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminV2Controller : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminV2Controller(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET api/adminv2/users
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<AdminV2UserListResponse>>> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var usersQuery = _userManager.Users
                .Include(u => u.Governorate)
                .Include(u => u.Branch)
                .Include(u => u.Appointments)
                .AsQueryable();

            // 1. Filter by Role if specified
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole.Select(u => u.Id).ToList();
                usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
            }

            // 2. Search filter
            if (!string.IsNullOrEmpty(search))
            {
                var cleanSearch = search.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.ToLower().Contains(cleanSearch) ||
                    u.LastName.ToLower().Contains(cleanSearch) ||
                    u.Email != null && u.Email.ToLower().Contains(cleanSearch) ||
                    u.NationalId.Contains(cleanSearch));
            }

            var totalCount = await usersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var users = await usersQuery
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = new List<AdminV2UserDto>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new AdminV2UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    NationalId = user.NationalId,
                    DateOfBirth = user.DateOfBirth,
                    Role = userRoles.FirstOrDefault() ?? Roles.Citizen,
                    GovernorateName = user.Governorate?.Name ?? "غير محدد",
                    BranchName = user.Branch?.Name ?? "غير محدد",
                    AppointmentsCount = user.Appointments.Count
                });
            }

            var response = new AdminV2UserListResponse
            {
                Users = userDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(ApiResponse<AdminV2UserListResponse>.Ok(response));
        }

        // GET api/adminv2/users/{id}
        [HttpGet("users/{id}")]
        public async Task<ActionResult<ApiResponse<AdminV2UserDetailDto>>> GetUserDetail(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.Governorate)
                .Include(u => u.Branch)
                .Include(u => u.Appointments)
                    .ThenInclude(a => a.ServiceType)
                .Include(u => u.Appointments)
                    .ThenInclude(a => a.Branch)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(ApiResponse<AdminV2UserDetailDto>.Fail("المستخدم غير موجود"));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var detailDto = new AdminV2UserDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                NationalId = user.NationalId,
                DateOfBirth = user.DateOfBirth,
                Role = roles.FirstOrDefault() ?? Roles.Citizen,
                GovernorateName = user.Governorate?.Name ?? "غير محدد",
                Area = user.Area ?? string.Empty,
                BranchName = user.Branch?.Name ?? "غير محدد",
                Appointments = user.Appointments
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.TimeSlot)
                    .Select(a => new AdminV2UserAppointmentDto
                    {
                        Id = a.Id,
                        AppointmentDate = a.AppointmentDate,
                        TimeSlot = a.TimeSlot,
                        ServiceName = a.ServiceType.Name,
                        BranchName = a.Branch.Name,
                        Status = a.Status.ToString()
                    })
                    .ToList()
            };

            return Ok(ApiResponse<AdminV2UserDetailDto>.Ok(detailDto));
        }

        // POST api/adminv2/users/{id}/reset-password
        [HttpPost("users/{id}/reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword(string id, [FromBody] AdminV2ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail("بيانات كلمة المرور غير صالحة"));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<string>.Fail("المستخدم غير موجود"));
            }

            // Remove password first
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                return BadRequest(ApiResponse<string>.Fail("فشل إزالة كلمة المرور القديمة", 
                    removeResult.Errors.Select(e => e.Description).ToList()));
            }

            // Add new password
            var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
            if (!addResult.Succeeded)
            {
                return BadRequest(ApiResponse<string>.Fail("فشل تعيين كلمة المرور الجديدة", 
                    addResult.Errors.Select(e => e.Description).ToList()));
            }

            return Ok(ApiResponse<string>.Ok(string.Empty, "تم إعادة تعيين كلمة المرور بنجاح"));
        }

        // GET api/adminv2/operators
        [HttpGet("operators")]
        public async Task<ActionResult<ApiResponse<object>>> GetOperators()
        {
            var operators = await _context.Operators
                .OrderBy(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.Key,
                    o.NameAr,
                    o.Emoji,
                    o.Color,
                    o.BgColor,
                    o.Hotline
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(operators));
        }

        // GET api/adminv2/operator-services
        [HttpGet("operator-services")]
        public async Task<ActionResult<ApiResponse<object>>> GetOperatorServices([FromQuery] int? operatorId)
        {
            var query = _context.OperatorServices.AsQueryable();
            if (operatorId.HasValue)
            {
                query = query.Where(s => s.OperatorId == operatorId.Value);
            }

            var services = await query
                .OrderBy(s => s.ServiceKey)
                .Select(s => new
                {
                    s.Id,
                    s.OperatorId,
                    s.ServiceKey,
                    s.NameAr,
                    s.Icon,
                    s.EstimatedTime
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(services));
        }

        // POST api/adminv2/operator-services
        [HttpPost("operator-services")]
        public async Task<ActionResult<ApiResponse<object>>> CreateOperatorService([FromBody] CreateOperatorServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("بيانات غير صالحة"));
            }

            var op = await _context.Operators.FindAsync(dto.OperatorId);
            if (op == null)
            {
                return NotFound(ApiResponse<object>.Fail("الجهة/المشغل غير موجود"));
            }

            var service = new OperatorService
            {
                OperatorId = dto.OperatorId,
                ServiceKey = dto.ServiceKey,
                Icon = dto.Icon ?? "bi-bookmark",
                NameAr = dto.NameAr,
                EstimatedTime = dto.EstimatedTime ?? "15 دقيقة"
            };

            _context.OperatorServices.Add(service);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(service, "تم إضافة الخدمة بنجاح"));
        }

        // PUT api/adminv2/operator-services/{id}
        [HttpPut("operator-services/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateOperatorService(int id, [FromBody] CreateOperatorServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("بيانات غير صالحة"));
            }

            var service = await _context.OperatorServices.FindAsync(id);
            if (service == null)
            {
                return NotFound(ApiResponse<object>.Fail("الخدمة غير موجودة"));
            }

            service.ServiceKey = dto.ServiceKey;
            if (dto.Icon != null) service.Icon = dto.Icon;
            service.NameAr = dto.NameAr;
            if (dto.EstimatedTime != null) service.EstimatedTime = dto.EstimatedTime;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(service, "تم تعديل الخدمة بنجاح"));
        }

        // DELETE api/adminv2/operator-services/{id}
        [HttpDelete("operator-services/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteOperatorService(int id)
        {
            var service = await _context.OperatorServices.FindAsync(id);
            if (service == null)
            {
                return NotFound(ApiResponse<string>.Fail("الخدمة غير موجودة"));
            }

            _context.OperatorServices.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok(string.Empty, "تم حذف الخدمة بنجاح"));
        }

        // GET api/adminv2/districts
        [HttpGet("districts")]
        public async Task<ActionResult<ApiResponse<object>>> GetDistricts([FromQuery] int? governorateId)
        {
            var query = _context.Districts.AsQueryable();
            if (governorateId.HasValue)
            {
                query = query.Where(d => d.GovernorateId == governorateId.Value);
            }

            var districts = await query
                .OrderBy(d => d.NameAr)
                .Select(d => new
                {
                    d.Id,
                    d.NameAr,
                    d.Type,
                    d.GovernorateId
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(districts));
        }
    }
}
