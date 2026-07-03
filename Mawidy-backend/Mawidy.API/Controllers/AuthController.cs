using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.DTOs.Auth;
using Mawidy.Application.DTOs.Common;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Mawidy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService IJwtService,
            IEmailService IEmailService,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = IJwtService;
            _emailService = IEmailService;
            _context = context;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("?????? ??? ?????",
                    ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList()));

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(ApiResponse<string>.Fail("??????? ?? ???? ??? ???"));

            var governorate = await _context.Governorates.FindAsync(dto.GovernorateId);
            if (governorate == null)
                return BadRequest(ApiResponse<string>.Fail("???????? ??? ??????"));

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                NationalId = dto.NationalId,
                DateOfBirth = dto.DateOfBirth,
                GovernorateId = dto.GovernorateId,
                Area = dto.Area
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.Fail("??? ????? ??????",
                    result.Errors.Select(e => e.Description).ToList()));

            await _userManager.AddToRoleAsync(user, Roles.Citizen);

            // ???? ????? ?????
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            try
            {
                await _emailService.SendEmailConfirmationAsync(user, token);
            }
            catch { }

            return Ok(ApiResponse<string>.Ok("", "?? ????? ?????? ?????? ???? ?? ??????"));
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Governorate)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("????? ?? ???? ???? ???"));

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("???? ???? ?????? ?????"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("????? ?? ???? ???? ???"));

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? Roles.Citizen,
                GovernorateId = user.GovernorateId,
                GovernorateName = user.Governorate?.Name,
                Area = user.Area,
                BranchId = user.BranchId,
                PhoneNumber = user.PhoneNumber
            }));
        }

        // POST api/auth/confirm-email
        [HttpPost("confirm-email")]
        public async Task<ActionResult<ApiResponse<string>>> ConfirmEmail(
            string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("???????? ??? ?????"));

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.Fail("???? ??????? ??? ?? ?????"));

            return Ok(ApiResponse<string>.Ok("", "?? ????? ??????? ?????"));
        }

        // POST api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                try
                {
                    await _emailService.SendPasswordResetAsync(user, token);
                }
                catch { }
            }

            return Ok(ApiResponse<string>.Ok("",
                "?? ??????? ????? ?????? ???? ?????? ????? ???? ??????"));
        }

        // POST api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword(
            string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("???????? ??? ?????"));

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.Fail("??? ????? ???? ??????",
                    result.Errors.Select(e => e.Description).ToList()));

            return Ok(ApiResponse<string>.Ok("", "?? ????? ???? ?????? ?????"));
        }

        // GET api/auth/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users
                .Include(u => u.Governorate)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(ApiResponse<AuthResponseDto>.Fail("???????? ??? ?????"));

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? Roles.Citizen,
                GovernorateId = user.GovernorateId,
                GovernorateName = user.Governorate?.Name,
                Area = user.Area,
                BranchId = user.BranchId
            }));
        }

        // PUT api/auth/profile
        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProfile(
            [FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(ApiResponse<string>.Fail("???????? ??? ?????"));

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.GovernorateId = dto.GovernorateId;
            user.Area = dto.Area;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "?? ????? ???????? ?????"));
        }


    }
}


