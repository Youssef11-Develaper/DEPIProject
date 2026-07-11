using Mawidy.Application.Hospitals.ViewModels;
using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.API.Hubs.Banks;
using Mawidy.API.Hubs.Hospitals;
using Mawidy.Application.Banks.Services;


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Mawidy.API.Controllers.Hospitals
{
    [Area("Hospitals")]
    public class AuthController : Controller
    {
        private readonly UserManager<Mawidy.Domain.Entities.ApplicationUser> _userManager;
        private readonly SignInManager<Mawidy.Domain.Entities.ApplicationUser> _signInManager;

        public AuthController(
            UserManager<Mawidy.Domain.Entities.ApplicationUser> userManager,
            SignInManager<Mawidy.Domain.Entities.ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

                // LOGIN
            public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Maw3dyCare");

            return View(new AdminLoginVM());
        }

              [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginVM vm)
        {
         
            if (!ModelState.IsValid)
                return View(vm);

         
            var admin = await _userManager.FindByEmailAsync(vm.Email);

            if (admin == null || !admin.IsActive)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(
                admin,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                admin.LastLoginAt = DateTime.Now;
                await _userManager.UpdateAsync(admin);

                return RedirectToAction("Index", "Maw3dyCare");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(vm);
        }

        // REGISTER

        public IActionResult Register()
        {
            return View(new AdminRegisterVM());
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AdminRegisterVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var existing = await _userManager.FindByEmailAsync(vm.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "This email is already registered");
                return View(vm);
            }

            var admin = new Mawidy.Domain.Entities.ApplicationUser
            {
                FirstName = vm.FullName,
                UserName = vm.Email,
                Email = vm.Email,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(admin, vm.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(admin, isPersistent: false);
                admin.LastLoginAt = DateTime.Now;
                await _userManager.UpdateAsync(admin);

                return RedirectToAction("Index", "Maw3dyCare");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }

        // LOGOUT

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}


