using System.Security.Claims;
using Mawidy.Application.Hospitals.ViewModels;
using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.API.Hubs.Banks;
using Mawidy.API.Hubs.Hospitals;
using Mawidy.Application.Banks.Services;
using Mawidy.Domain.Entities;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Mawidy.API.Controllers.Hospitals
{
    [Area("Hospitals")]
    public class Maw3dyCareController : Controller
    {
        private readonly Mawidy.Infrastructure.Persistence.AppDbContext _db;
        private readonly UserManager<Mawidy.Domain.Entities.ApplicationUser> _userManager;

        public Maw3dyCareController(
            Mawidy.Infrastructure.Persistence.AppDbContext db,
            UserManager<Mawidy.Domain.Entities.ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }





        [AllowAnonymous]
        public IActionResult Landing()
        {
            return View();
        }



        [AllowAnonymous]
        public IActionResult Hospitals()
        {
            var hospitals = _db.Hospitals
                .Where(h => h.IsActive)
                .Include(h => h.Beds)
                    .ThenInclude(b => b.BedTypes)
                .ToList();

            return View(hospitals);
        }
        public IActionResult HospitalDetails(int id)
        {
            var hospital = _db.Hospitals
                .Where(h => h.HospitalId == id && h.IsActive)
                .Include(h => h.Beds)
                    .ThenInclude(b => b.BedTypes)
                .FirstOrDefault();

            if (hospital == null)
                return NotFound();

            return View(hospital);
        }

        [AllowAnonymous]
        public IActionResult Reserve(int id)
        {
            var hospital = _db.Hospitals
                .Include(h => h.Beds)
                    .ThenInclude(b => b.BedTypes)
                .FirstOrDefault(h => h.HospitalId == id);

            if (hospital == null)
                return NotFound();

            ViewBag.HospitalName = hospital.Name;
            ViewBag.HospitalId = hospital.HospitalId;
            ViewBag.BedTypes = hospital.Beds
                .Where(b => b.Status == "Available")
                .Select(b => b.BedTypes)
                .Distinct()
                .ToList();

            return View();
        }

       
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Reserve(Reservations reservation)
        {
            bool isBlocked = _db.HospitalBlockedPhones.Any(b =>
                b.HospitalId == reservation.HospitalId &&
                b.Phone == reservation.PatientPhone);

            if (isBlocked)
            {
                ModelState.AddModelError(
                    "PatientPhone",
                    "This number is blocked and cannot make reservations.");
            }

            reservation.Status = "Pending";
            reservation.ReservedAt = DateTime.Now;
            reservation.ExpiresAt = DateTime.Now.AddMinutes(30);

            // Link to Mawidy account if user is logged in via SSO
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                reservation.UserId = userId;

            ModelState.Remove("Hospitals");
            ModelState.Remove("BedTypes");
            ModelState.Remove("QueueTickets");
            ModelState.Remove("Status");
            ModelState.Remove("Bed");

            if (!ModelState.IsValid)
            {
                var hospital = _db.Hospitals
                    .Include(h => h.Beds)
                        .ThenInclude(b => b.BedTypes)
                    .FirstOrDefault(h => h.HospitalId == reservation.HospitalId);

                ViewBag.HospitalName = hospital?.Name;
                ViewBag.HospitalId = hospital?.HospitalId;
                ViewBag.BedTypes = hospital?.Beds
                    .Where(b => b.Status == "Available")
                    .Select(b => b.BedTypes)
                    .Distinct()
                    .ToList();

                return View(reservation);
            }

            _db.HospitalReservations.Add(reservation);
            _db.SaveChanges();

            return RedirectToAction("Confirmation",
                new { id = reservation.ReservationId });
        }

       
        public IActionResult Confirmation(int id)
        {
            var reservation = _db.HospitalReservations
                .Include(r => r.BedTypes)
                .Include(r => r.Hospitals)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

       
        [AllowAnonymous]
        public IActionResult Status(int id)
        {
            var reservation = _db.HospitalReservations
                .Include(r => r.BedTypes)
                .Include(r => r.Hospitals)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

       
        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Redirect unauthenticated users to Landing page
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Landing");

            var hospitals = await _db.Hospitals
                .Include(h => h.Beds)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalHospitals = hospitals.Count,
                TotalBeds = hospitals.Sum(h => h.Beds?.Count ?? 0),
                AvailableBeds = hospitals.Sum(h => h.Beds?
                                 .Count(b => b.Status == "Available") ?? 0),
                OccupiedBeds = hospitals.Sum(h => h.Beds?
                                 .Count(b => b.Status == "Occupied") ?? 0),
                Hospitals = hospitals.Select(h => new HospitalCardViewModel
                {
                    HospitalId = h.HospitalId,
                    Name = h.Name,
                    City = h.City,
                    Address = h.Address,
                    IsActive = h.IsActive,
                    TotalBeds = h.Beds?.Count ?? 0,
                    AvailableBeds = h.Beds?.Count(b => b.Status == "Available") ?? 0
                }).ToList()
            };

            return View(vm);
        }

      
        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [AllowAnonymous]
        public IActionResult HospitalsList(string? search)
        {
            var query = _db.Hospitals
                .Include(h => h.Beds)
                .ThenInclude(b => b.BedTypes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(h =>
                    h.Name.Contains(search) ||
                    h.City.Contains(search));

            var vm = new HospitalsPageVM
            {
                SearchTerm = search,
                Hospitals = query.Select(h => new HospitalListItemVM
                {
                    HospitalId = h.HospitalId,
                    Name = h.Name,
                    City = h.City,
                    Address = h.Address,
                    Phone = h.Phone,
                    Email = h.Email,
                    IsActive = h.IsActive,
                    TotalBeds = h.Beds.Count,
                    AvailableBeds = h.Beds.Count(b => b.Status == "Available"),
                    OccupiedBeds = h.Beds.Count(b => b.Status == "Occupied")
                }).ToList()
            };

            return View(vm);
        }

      
        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public IActionResult HospitalDetail(int id)
        {
            var h = _db.Hospitals
                .Include(h => h.Beds)
                .ThenInclude(b => b.BedTypes)
                .FirstOrDefault(h => h.HospitalId == id);

            if (h == null) return NotFound();

            var vm = new HospitalDetailsVM
            {
                HospitalId = h.HospitalId,
                Name = h.Name,
                City = h.City,
                Address = h.Address,
                Phone = h.Phone,
                Email = h.Email,
                Description = h.Description,
                Latitude = h.Latitude,
                Longitude = h.Longitude,
                IsActive = h.IsActive,
                CreatedAt = h.CreatedAt,
                BedTypeStatuses = h.Beds
                    .GroupBy(b => b.BedTypes!.Name)
                    .Select(g => new BedTypeStatusVM
                    {
                        TypeName = g.Key,
                        Total = g.Count(),
                        Available = g.Count(b => b.Status == "Available"),
                        Occupied = g.Count(b => b.Status == "Occupied")
                    }).ToList()
            };

            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [HttpPost]
        public IActionResult ToggleHospital(int id)
        {
            var h = _db.Hospitals.Find(id);
            if (h == null) return NotFound();
            h.IsActive = !h.IsActive;
            _db.SaveChanges();
            return RedirectToAction("HospitalsList");
        }

      
        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public IActionResult AddHospital()
        {
            return View(new AddHospitalVM());
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [HttpPost]
        public IActionResult AddHospital(AddHospitalVM vm)
        {
            ModelState.Remove("Beds");
            ModelState.Remove("Reservations");
            ModelState.Remove("Mawidy.Domain.Entities.ApplicationUser");

            if (!ModelState.IsValid)
                return View(vm);

            var hospital = new Mawidy.Domain.Entities.Hospitals.Hospitals
            {
                Name = vm.Name,
                Address = vm.Address,
                City = vm.City,
                Phone = vm.Phone,
                Email = vm.Email ?? "",
                Description = vm.Description,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.Now
            };

            _db.Hospitals.Add(hospital);
            _db.SaveChanges();

            var bedTypes = _db.HospitalBedTypes.ToList();
            int icuId = bedTypes.First(t => t.Name == "ICU").BedTypeId;
            int nicuId = bedTypes.First(t => t.Name == "NICU").BedTypeId;
            int cicuId = bedTypes.First(t => t.Name == "CICU").BedTypeId;
            int ventId = bedTypes.First(t => t.Name == "VENT").BedTypeId;

            var beds = new List<Beds>();
            beds.AddRange(GenerateBeds("ICU", icuId, hospital.HospitalId, vm.IcuAvailable, vm.IcuOccupied));
            beds.AddRange(GenerateBeds("NICU", nicuId, hospital.HospitalId, vm.NicuAvailable, vm.NicuOccupied));
            beds.AddRange(GenerateBeds("CICU", cicuId, hospital.HospitalId, vm.CicuAvailable, vm.CicuOccupied));
            beds.AddRange(GenerateBeds("VENT", ventId, hospital.HospitalId, vm.VentAvailable, vm.VentOccupied));

            if (beds.Any())
            {
                _db.HospitalBeds.AddRange(beds);
                _db.SaveChanges();
            }

            return RedirectToAction("CreateHospitalAccount",
                new { hospitalId = hospital.HospitalId, email = vm.Email, name = vm.Name });
        }

        private List<Beds> GenerateBeds(
            string prefix, int typeId, int hospitalId,
            int availableCount, int occupiedCount)
        {
            var beds = new List<Beds>();
            int counter = 1;

            for (int i = 0; i < availableCount; i++)
                beds.Add(new Beds
                {
                    BedNumber = $"{prefix}-{counter++:D2}",
                    Status = "Available",
                    HospitalId = hospitalId,
                    BedTypeId = typeId
                });

            for (int i = 0; i < occupiedCount; i++)
                beds.Add(new Beds
                {
                    BedNumber = $"{prefix}-{counter++:D2}",
                    Status = "Occupied",
                    HospitalId = hospitalId,
                    BedTypeId = typeId
                });

            return beds;
        }

        
        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public IActionResult CreateHospitalAccount(int hospitalId, string? email, string? name)
        {
            var vm = new CreateHospitalAccountVM
            {
                HospitalId = hospitalId,
                HospitalName = name ?? _db.Hospitals.Find(hospitalId)?.Name ?? "",
                Email = email ?? "",
                FullName = name ?? ""
            };
            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [HttpPost]
        public IActionResult CreateHospitalAccount(CreateHospitalAccountVM vm)
        {
            ModelState.Remove("HospitalName");

            if (!ModelState.IsValid)
                return View(vm);

            bool emailExists = _db.Users.Any(u => u.Email == vm.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered");
                return View(vm);
            }

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Mawidy.Domain.Entities.ApplicationUser>();
            var user = new Mawidy.Domain.Entities.ApplicationUser
            {
                FirstName = vm.FullName,
                Email = vm.Email,
                HospitalId = vm.HospitalId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            user.PasswordHash = hasher.HashPassword(user, vm.Password);

            _db.Users.Add(user);
            _db.SaveChanges();

            TempData["AccountSuccess"] = $"? Account created for {vm.FullName}";
            return RedirectToAction("HospitalsList");
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [HttpPost]
        public IActionResult SkipAccount()
        {
            return RedirectToAction("HospitalsList");
        }

       
        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public IActionResult Accounts()
        {
            var users = _db.Users
                .Include(u => u.Hospital)
                .Select(u => new AccountCardVM
                {
                    UserId = u.Id,
                    FullName = u.FirstName,
                    Email = u.Email,
                    HospitalName = u.Hospital != null ? u.Hospital.Name : "�",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

            return View(users);
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public IActionResult ResetPassword(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            var vm = new ResetPasswordVM
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVM vm)
        {
            ModelState.Remove("FullName");
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
                return View(vm);

            var user = _db.Users.Find(vm.UserId);
            if (user == null) return NotFound();

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Mawidy.Domain.Entities.ApplicationUser>();
            user.PasswordHash = hasher.HashPassword(user, vm.NewPassword);
            _db.SaveChanges();

            TempData["Success"] = $"? Password updated for {user.FullName}";
            return RedirectToAction("Accounts");
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [HttpPost]
        public IActionResult ToggleUserActive(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            _db.SaveChanges();

            TempData["Success"] = $"{(user.IsActive ? "? Enabled" : "? Disabled")} � {user.FullName}";
            return RedirectToAction("Accounts");
        }

             [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public async Task<IActionResult> Settings()
        {
            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
                return RedirectToAction("Login", "Auth");

            var vm = new SettingsVM
            {
                AdminId = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                IsActive = admin.IsActive,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt
            };

            return View(vm);
        }



        
        public IActionResult Report()
        {
            ViewBag.Hospitals = _db.Hospitals
                .Where(h => h.IsActive)
                .Select(h => h.Name)
                .ToList();

            return View(new ReportVM());
        }

       
        [HttpPost]
        public IActionResult Report(ReportVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Hospitals = _db.Hospitals
                    .Where(h => h.IsActive)
                    .Select(h => h.Name)
                    .ToList();
                return View(vm);
            }

            var report = new Reports
            {
                PatientName = vm.PatientName,
                PatientPhone = vm.PatientPhone,
                HospitalName = vm.HospitalName,
                Complaint = vm.Complaint,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.HospitalReports.Add(report);
            _db.SaveChanges();

            TempData["ReportSuccess"] = "true";
            return RedirectToAction("ReportConfirm");
        }

       
        public IActionResult ReportConfirm()
        {
            return View();
        }

       
        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public IActionResult Reports()
        {
            var reports = _db.HospitalReports
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReportListVM
                {
                    ReportId = r.ReportId,
                    PatientName = r.PatientName,
                    PatientPhone = r.PatientPhone,
                    HospitalName = r.HospitalName,
                    Complaint = r.Complaint,
                    IsRead = r.IsRead,
                    CreatedAt = r.CreatedAt
                }).ToList();

            return View(reports);
        }

        [Authorize(AuthenticationSchemes = "HospitalCookies")]
        [HttpPost]
        public IActionResult MarkReportRead(int id)
        {
            var report = _db.HospitalReports.Find(id);
            if (report != null)
            {
                report.IsRead = true;
                _db.SaveChanges();
            }
            return RedirectToAction("Reports");
        }

               [Authorize(AuthenticationSchemes = "HospitalCookies")]
        public IActionResult GetUnreadReports()
        {
            var unread = _db.HospitalReports
                .Where(r => !r.IsRead)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new
                {
                    id = r.ReportId,
                    patientName = r.PatientName,
                    hospitalName = r.HospitalName,
                    createdAt = r.CreatedAt.ToString("MMM dd, hh:mm tt")
                }).ToList();

            return Json(new
            {
                count = _db.HospitalReports.Count(r => !r.IsRead),
                reports = unread
            });
        }

         





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}





