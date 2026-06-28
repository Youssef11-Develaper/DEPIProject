using Maw3dyCare.Models;
using Maw3dyCare.Models.Maw3dyCareDB;
using Maw3dyCare.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Maw3dyCare.Controllers
{
    public class Maw3dyCareController : Controller
    {
        private readonly Maw3dyCareDB _db;
        private readonly UserManager<ApplicationAdmin> _userManager;

        public Maw3dyCareController(
            Maw3dyCareDB db,
            UserManager<ApplicationAdmin> userManager)
        {
            _db = db;
            _userManager = userManager;
        }





        public IActionResult Landing()
        {
            return View();
        }



        public IActionResult Hospitals()
        {
            var hospitals = _db.Hospital
                .Where(h => h.IsActive)
                .Include(h => h.Beds)
                    .ThenInclude(b => b.BedTypes)
                .ToList();

            return View(hospitals);
        }
        public IActionResult HospitalDetails(int id)
        {
            var hospital = _db.Hospital
                .Where(h => h.HospitalId == id && h.IsActive)
                .Include(h => h.Beds)
                    .ThenInclude(b => b.BedTypes)
                .FirstOrDefault();

            if (hospital == null)
                return NotFound();

            return View(hospital);
        }

        public IActionResult Reserve(int id)
        {
            var hospital = _db.Hospital
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
        public IActionResult Reserve(Reservations reservation)
        {
            bool isBlocked = _db.BlockedPhone.Any(b =>
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

            ModelState.Remove("Hospitals");
            ModelState.Remove("BedTypes");
            ModelState.Remove("QueueTickets");
            ModelState.Remove("Status");
            ModelState.Remove("Bed");

            if (!ModelState.IsValid)
            {
                var hospital = _db.Hospital
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

            _db.Reservation.Add(reservation);
            _db.SaveChanges();

            return RedirectToAction("Confirmation",
                new { id = reservation.ReservationId });
        }

       
        public IActionResult Confirmation(int id)
        {
            var reservation = _db.Reservation
                .Include(r => r.BedTypes)
                .Include(r => r.Hospitals)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

       
        public IActionResult Status(int id)
        {
            var reservation = _db.Reservation
                .Include(r => r.BedTypes)
                .Include(r => r.Hospitals)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

       
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var hospitals = await _db.Hospital
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

      
        [Authorize]
        public IActionResult HospitalsList(string? search)
        {
            var query = _db.Hospital
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

      
        [Authorize]
        public IActionResult HospitalDetail(int id)
        {
            var h = _db.Hospital
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

        [Authorize]
        [HttpPost]
        public IActionResult ToggleHospital(int id)
        {
            var h = _db.Hospital.Find(id);
            if (h == null) return NotFound();
            h.IsActive = !h.IsActive;
            _db.SaveChanges();
            return RedirectToAction("HospitalsList");
        }

      
        [Authorize]
        public IActionResult AddHospital()
        {
            return View(new AddHospitalVM());
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddHospital(AddHospitalVM vm)
        {
            ModelState.Remove("Beds");
            ModelState.Remove("Reservations");
            ModelState.Remove("HospitalUsers");

            if (!ModelState.IsValid)
                return View(vm);

            var hospital = new Hospitals
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

            _db.Hospital.Add(hospital);
            _db.SaveChanges();

            var bedTypes = _db.BedType.ToList();
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
                _db.Bed.AddRange(beds);
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

        
        [Authorize]
        public IActionResult CreateHospitalAccount(int hospitalId, string? email, string? name)
        {
            var vm = new CreateHospitalAccountVM
            {
                HospitalId = hospitalId,
                HospitalName = name ?? _db.Hospital.Find(hospitalId)?.Name ?? "",
                Email = email ?? "",
                FullName = name ?? ""
            };
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateHospitalAccount(CreateHospitalAccountVM vm)
        {
            ModelState.Remove("HospitalName");

            if (!ModelState.IsValid)
                return View(vm);

            bool emailExists = _db.HospitalUser.Any(u => u.Email == vm.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered");
                return View(vm);
            }

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<HospitalUsers>();
            var user = new HospitalUsers
            {
                FullName = vm.FullName,
                Email = vm.Email,
                HospitalId = vm.HospitalId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            user.PasswordHash = hasher.HashPassword(user, vm.Password);

            _db.HospitalUser.Add(user);
            _db.SaveChanges();

            TempData["AccountSuccess"] = $"✓ Account created for {vm.FullName}";
            return RedirectToAction("HospitalsList");
        }

        [Authorize]
        [HttpPost]
        public IActionResult SkipAccount()
        {
            return RedirectToAction("HospitalsList");
        }

       
        [Authorize]
        public IActionResult Accounts()
        {
            var users = _db.HospitalUser
                .Include(u => u.Hospitals)
                .Select(u => new AccountCardVM
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    HospitalName = u.Hospitals != null ? u.Hospitals.Name : "—",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

            return View(users);
        }

        [Authorize]
        public IActionResult ResetPassword(int id)
        {
            var user = _db.HospitalUser.Find(id);
            if (user == null) return NotFound();

            var vm = new ResetPasswordVM
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email
            };
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVM vm)
        {
            ModelState.Remove("FullName");
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
                return View(vm);

            var user = _db.HospitalUser.Find(vm.UserId);
            if (user == null) return NotFound();

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<HospitalUsers>();
            user.PasswordHash = hasher.HashPassword(user, vm.NewPassword);
            _db.SaveChanges();

            TempData["Success"] = $"✓ Password updated for {user.FullName}";
            return RedirectToAction("Accounts");
        }

        [Authorize]
        [HttpPost]
        public IActionResult ToggleUserActive(int id)
        {
            var user = _db.HospitalUser.Find(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            _db.SaveChanges();

            TempData["Success"] = $"{(user.IsActive ? "✓ Enabled" : "✓ Disabled")} — {user.FullName}";
            return RedirectToAction("Accounts");
        }

             [Authorize]
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
            ViewBag.Hospitals = _db.Hospital
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
                ViewBag.Hospitals = _db.Hospital
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

            _db.Report.Add(report);
            _db.SaveChanges();

            TempData["ReportSuccess"] = "true";
            return RedirectToAction("ReportConfirm");
        }

       
        public IActionResult ReportConfirm()
        {
            return View();
        }

       
        [Authorize]
        public IActionResult Reports()
        {
            var reports = _db.Report
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

        [Authorize]
        [HttpPost]
        public IActionResult MarkReportRead(int id)
        {
            var report = _db.Report.Find(id);
            if (report != null)
            {
                report.IsRead = true;
                _db.SaveChanges();
            }
            return RedirectToAction("Reports");
        }

               [Authorize]
        public IActionResult GetUnreadReports()
        {
            var unread = _db.Report
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
                count = _db.Report.Count(r => !r.IsRead),
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