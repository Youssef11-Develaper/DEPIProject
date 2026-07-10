using Mawidy.Application.Hospitals.ViewModels;
using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.API.Hubs.Banks;
using Mawidy.API.Hubs.Hospitals;
using Mawidy.Application.Banks.Services;


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mawidy.API.Controllers.Hospitals
{
    [Area("Hospitals")]
    public class HospitalAuthController : Controller
    {
        private readonly Mawidy.Infrastructure.Persistence.AppDbContext _db;

        public HospitalAuthController(Mawidy.Infrastructure.Persistence.AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true &&
                User.Identity.AuthenticationType == "HospitalCookie")
                return RedirectToAction("Index", "HospitalDashboard");

            return View(new HospitalLoginVM());
        }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(HospitalLoginVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = _db.Users
                .FirstOrDefault(u => u.Email == vm.Email && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(vm);
            }

            var hasher = new PasswordHasher<Mawidy.Domain.Entities.ApplicationUser>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,  user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("HospitalId",     user.HospitalId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "HospitalCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("HospitalCookie", principal);

            return RedirectToAction("Index", "HospitalDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("HospitalCookie");
            return RedirectToAction("Login");
        }
    }
}


