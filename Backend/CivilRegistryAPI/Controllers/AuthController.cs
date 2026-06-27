using CivilRegistryAPI.Data;
using CivilRegistryAPI.DTOs.Auth;
using CivilRegistryAPI.DTOs.Common;
using CivilRegistryAPI.Models;
using CivilRegistryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CivilRegistryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            EmailService emailService,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _context = context;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("بيانات غير صحيحة",
                    ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList()));

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(ApiResponse<string>.Fail("الإيميل ده مسجل قبل كده"));

            var governorate = await _context.Governorates.FindAsync(dto.GovernorateId);
            if (governorate == null)
                return BadRequest(ApiResponse<string>.Fail("المحافظة غير موجودة"));

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
                return BadRequest(ApiResponse<string>.Fail("فشل إنشاء الحساب",
                    result.Errors.Select(e => e.Description).ToList()));

            await _userManager.AddToRoleAsync(user, Roles.Citizen);

            // ارسل إيميل تأكيد
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            try
            {
                await _emailService.SendEmailConfirmationAsync(user, token);
            }
            catch { }

            return Ok(ApiResponse<string>.Ok("", "تم إنشاء الحساب بنجاح، تحقق من إيميلك"));
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Governorate)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("إيميل أو كلمة مرور غلط"));

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("لازم تأكد إيميلك الأول"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("إيميل أو كلمة مرور غلط"));

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
                BranchId = user.BranchId
            }));
        }

        // POST api/auth/confirm-email
        [HttpPost("confirm-email")]
        public async Task<ActionResult<ApiResponse<string>>> ConfirmEmail(
            string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("المستخدم غير موجود"));

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.Fail("رابط التأكيد غلط أو منتهي"));

            return Ok(ApiResponse<string>.Ok("", "تم تأكيد الإيميل بنجاح"));
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
                "لو الإيميل موجود هيوصلك رابط لإعادة تعيين كلمة المرور"));
        }

        // POST api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword(
            string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("المستخدم غير موجود"));

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.Fail("فشل تغيير كلمة المرور",
                    result.Errors.Select(e => e.Description).ToList()));

            return Ok(ApiResponse<string>.Ok("", "تم تغيير كلمة المرور بنجاح"));
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
                return NotFound(ApiResponse<AuthResponseDto>.Fail("المستخدم غير موجود"));

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
                return NotFound(ApiResponse<string>.Fail("المستخدم غير موجود"));

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.GovernorateId = dto.GovernorateId;
            user.Area = dto.Area;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("", "تم تحديث البيانات بنجاح"));
        }


    }
}
