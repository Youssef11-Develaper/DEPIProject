using Maw3dyCare.Models.Maw3dyCareDB;
using Maw3dyCare.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Maw3dyCare.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationAdmin> _userManager;
        private readonly SignInManager<ApplicationAdmin> _signInManager;

        public AuthController(
            UserManager<ApplicationAdmin> userManager,
            SignInManager<ApplicationAdmin> signInManager)
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

            var admin = new ApplicationAdmin
            {
                FullName = vm.FullName,
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