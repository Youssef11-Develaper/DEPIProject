using Maw3dyCare.Models.Maw3dyCareDB;
using Maw3dyCare.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3dyCare.Controllers
{
    public class HospitalAuthController : Controller
    {
        private readonly Maw3dyCareDB _db;

        public HospitalAuthController(Maw3dyCareDB db)
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

            var user = _db.HospitalUser
                .FirstOrDefault(u => u.Email == vm.Email && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(vm);
            }

            var hasher = new PasswordHasher<HospitalUsers>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
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