using Mawidy.Application.Hospitals.ViewModels;
using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Mawidy.API.Hubs.Banks;
using Mawidy.API.Hubs.Hospitals;
using Mawidy.Application.Banks.Services;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;


namespace Mawidy.API.Controllers.Hospitals
{
    [Authorize(AuthenticationSchemes = "HospitalCookie")]
    [Area("Hospitals")]
    public class HospitalDashboardController : Controller
    {
        private readonly Mawidy.Infrastructure.Persistence.AppDbContext _db;
        private readonly IHubContext<ReservationHub> _hub;
        public HospitalDashboardController(
     Mawidy.Infrastructure.Persistence.AppDbContext db,
     IHubContext<ReservationHub> hub)
        {
            _db = db;
            _hub = hub;
        }
        // ============================================================
        // Helper: Get HospitalId safely
        // ============================================================
        private int GetHospitalId()
        {
            var val = User.FindFirst("HospitalId")?.Value;
            return int.TryParse(val, out int id) ? id : 0;
        }

        // ============================================================
        // Dashboard Home
        // ============================================================
        public IActionResult Index()
        {
            int hospitalId = GetHospitalId();
            if (hospitalId == 0)
                return RedirectToAction("Login", "HospitalAuth");

            var hospital = _db.Hospitals
                .Include(h => h.Beds!)
                    .ThenInclude(b => b.BedTypes!)
                .FirstOrDefault(h => h.HospitalId == hospitalId);

            if (hospital == null)
                return RedirectToAction("Login", "HospitalAuth");

            var allBeds = hospital.Beds?.ToList() ?? new();

            var availableBeds = allBeds.Where(b => b.Status == "Available").ToList();
            var occupiedBeds = allBeds.Where(b => b.Status == "Occupied").ToList();

            int pendingCount = _db.HospitalReservations
                .Count(r => r.HospitalId == hospitalId && r.Status == "Pending");

            var vm = new HospitalDashboardVM
            {
                HospitalName = hospital.Name,
                City = hospital.City,
                IsActive = hospital.IsActive,

                TotalBeds = allBeds.Count,
                AvailableBeds = availableBeds.Count,
                OccupiedBeds = occupiedBeds.Count,
                PendingCount = pendingCount,

                AvailableByType = availableBeds
                    .GroupBy(b => b.BedTypes?.Name ?? "Unknown")
                    .Select(g => new BedTypeSummary
                    {
                        TypeName = g.Key,
                        Count = g.Count()
                    }).ToList(),

                OccupiedByType = occupiedBeds
                    .GroupBy(b => b.BedTypes?.Name ?? "Unknown")
                    .Select(g => new BedTypeSummary
                    {
                        TypeName = g.Key,
                        Count = g.Count()
                    }).ToList()
            };

            return View(vm);
        }

        // ============================================================
        // Reservations (Pending)
        // ============================================================
        public IActionResult Reservations(string? search, string? bedType)
        {
            int hospitalId = GetHospitalId();

            var query = _db.HospitalReservations
                .Include(r => r.BedTypes)
                .Where(r => r.HospitalId == hospitalId && r.Status == "Pending")
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.PatientName.Contains(search) ||
                    r.PatientPhone.Contains(search));
            }

            if (!string.IsNullOrEmpty(bedType) && bedType != "All")
            {
                query = query.Where(r => r.BedTypes!.Name == bedType);
            }

            var reservations = query
                .OrderByDescending(r => r.ReservedAt)
                .Select(r => new ReservationListItemVM
                {
                    ReservationId = r.ReservationId,
                    PatientName = r.PatientName,
                    PatientPhone = r.PatientPhone,
                    BedTypeName = r.BedTypes != null ? r.BedTypes.Name : "—",
                    CaseDescription = r.CaseDescription,
                    Status = r.Status,
                    ETA = r.ETA,
                    ReservedAt = r.ReservedAt
                }).ToList();

            ViewBag.PendingCount = reservations.Count;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentBedType = bedType ?? "All";

            return View(reservations);
        }

        // ============================================================
        // Reservation Details (AJAX)
        // ============================================================
        public IActionResult ReservationDetail(int id)
        {
            int hospitalId = GetHospitalId();

            var r = _db.HospitalReservations
                .Include(x => x.BedTypes)
                .Include(x => x.Hospitals)
                .FirstOrDefault(x => x.ReservationId == id && x.HospitalId == hospitalId);

            if (r == null) return NotFound();

            return Json(new ReservationDetailVM
            {
                ReservationId = r.ReservationId,
                PatientName = r.PatientName,
                PatientPhone = r.PatientPhone,
                BedTypeName = r.BedTypes?.Name ?? "—",
                CaseDescription = r.CaseDescription,
                Status = r.Status,
                ETA = r.ETA,
                ReservedAt = r.ReservedAt,
                ExpiresAt = r.ExpiresAt,
                HospitalName = r.Hospitals?.Name ?? "—"
            });
        }

        // ============================================================
        // Accept Reservation
        //// ============================================================
        //[HttpPost]
        //public IActionResult AcceptReservation(int id)
        //{
        //    int hospitalId = GetHospitalId();

        //    var reservation = _db.HospitalReservations
        //        .Include(r => r.BedTypes)
        //        .FirstOrDefault(r => r.ReservationId == id && r.HospitalId == hospitalId);

        //    if (reservation == null)
        //        return Json(new { success = false, message = "Reservation not found" });

        //    var bed = _db.HospitalBeds.FirstOrDefault(b =>
        //        b.HospitalId == hospitalId &&
        //        b.BedTypeId == reservation.BedTypeId &&
        //        b.Status == "Available");

        //    if (bed == null)
        //        return Json(new { success = false, message = "No available bed" });

        //    bed.Status = "Occupied";
        //    reservation.Status = "Accepted";

        //    _db.SaveChanges();

        //    return Json(new
        //    {
        //        success = true,
        //        bedNumber = bed.BedNumber,
        //        message = $"Bed {bed.BedNumber} assigned"
        //    });
        //}

        [HttpPost]
        public async Task<IActionResult> AcceptReservation(int id)
        {
            int hospitalId = GetHospitalId();

            var reservation = _db.HospitalReservations
                .Include(r => r.BedTypes)
                .FirstOrDefault(r => r.ReservationId == id
                                  && r.HospitalId == hospitalId);

            if (reservation == null)
                return Json(new { success = false, message = "Reservation not found" });

            // ???? ?? ??? ???? ???? ?? ??? ?????
            var availableBed = _db.HospitalBeds
                .FirstOrDefault(b => b.HospitalId == hospitalId
                                  && b.BedTypeId == reservation.BedTypeId
                                  && b.Status == "Available");

            if (availableBed == null)
                return Json(new
                {
                    success = false,
                    message = $"No available {reservation.BedTypes?.Name} bed"
                });

            // ???? ?????? ?? Occupied
            availableBed.Status = "Occupied";

            // ? ???? ??? BedId ?? ?????
            reservation.Status = "Accepted";
            reservation.BedId = availableBed.BedId;

            _db.SaveChanges();
            await _hub.Clients.Group(id.ToString())
    .SendAsync("StatusChanged", "Accepted");

            return Json(new
            {
                success = true,
                bedNumber = availableBed.BedNumber,
                bedType = reservation.BedTypes?.Name,
                message = $"Bed {availableBed.BedNumber} assigned successfully"
            });
        }

        // ============================================================
        // Reject Reservation
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> RejectReservation(int id)
        {
            int hospitalId = GetHospitalId();

            var reservation = _db.HospitalReservations
                .FirstOrDefault(r => r.ReservationId == id && r.HospitalId == hospitalId);

            if (reservation == null)
                return Json(new { success = false });

            reservation.Status = "Rejected";
            _db.SaveChanges();
            await _hub.Clients.Group(id.ToString())
    .SendAsync("StatusChanged", "Rejected");

            return Json(new { success = true });
        }

        public IActionResult AddBed()
        {
            ViewBag.BedTypes = _db.HospitalBedTypes.ToList();
            return View(new AddBedVM());
        }

        [HttpPost]
        public IActionResult AddBed(AddBedVM vm)
        {
            int hospitalId = GetHospitalId();

            if (!ModelState.IsValid)
            {
                ViewBag.BedTypes = _db.HospitalBedTypes.ToList();
                return View(vm);
            }

            bool exists = _db.HospitalBeds.Any(b =>
                b.HospitalId == hospitalId &&
                b.BedNumber == vm.BedNumber);

            if (exists)
            {
                ModelState.AddModelError("", "Bed already exists");
                ViewBag.BedTypes = _db.HospitalBedTypes.ToList();
                return View(vm);
            }

            _db.HospitalBeds.Add(new Beds
            {
                BedNumber = vm.BedNumber,
                BedTypeId = vm.BedTypeId,
                HospitalId = hospitalId,
                Status = "Available"
            });

            _db.SaveChanges();

            return RedirectToAction("ManageBeds");
        }

              public IActionResult ManageBeds(string? search, string? filterType, string? filterStatus)
        {
            int hospitalId = GetHospitalId();

            var query = _db.HospitalBeds
                .Include(b => b.BedTypes)
                .Where(b => b.HospitalId == hospitalId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(b => b.BedNumber.Contains(search));

            if (!string.IsNullOrEmpty(filterType) && filterType != "All")
                query = query.Where(b => b.BedTypes!.Name == filterType);

            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "All")
                query = query.Where(b => b.Status == filterStatus);

            var beds = query.OrderBy(b => b.BedNumber).ToList();

            var acceptedReservations = _db.HospitalReservations
                .Where(r => r.HospitalId == hospitalId
                         && r.Status == "Accepted"
                         && r.BedId != null)
                .ToList();

            var bedToReservation = acceptedReservations
                .GroupBy(r => r.BedId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.ReservedAt).First());

            var vm = beds.Select(b => {
                bedToReservation.TryGetValue(b.BedId, out var matchedRes);

                return new ManageBedItemVM
                {
                    BedId = b.BedId,
                    BedNumber = b.BedNumber,
                    BedTypeName = b.BedTypes?.Name ?? "—",
                    BedTypeId = b.BedTypeId,
                    Status = b.Status,
                    AssignedPatient = b.Status == "Occupied"
                        ? (matchedRes?.PatientName ?? null)
                        : null,
                    PatientPhone = b.Status == "Occupied"
                        ? matchedRes?.PatientPhone
                        : null,
                    CaseDescription = b.Status == "Occupied"
                        ? matchedRes?.CaseDescription
                        : null
                };
            }).ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.FilterType = filterType ?? "All";
            ViewBag.FilterStatus = filterStatus ?? "All";

            return View(vm);
        }



        [HttpPost]
        public IActionResult EditBedStatus(int bedId, string newStatus)
        {
            int hospitalId = GetHospitalId();

            var bed = _db.HospitalBeds.FirstOrDefault(b => b.BedId == bedId && b.HospitalId == hospitalId);

            if (bed == null)
                return Json(new { success = false });

            bed.Status = newStatus;
            _db.SaveChanges();

            return Json(new { success = true });
        }

             public IActionResult History(string? search)
        {
            int hospitalId = GetHospitalId();

            var query = _db.HospitalReservations
                .Include(r => r.BedTypes)
                .Where(r => r.HospitalId == hospitalId
                         && (r.Status == "Accepted" || r.Status == "Rejected"))
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r =>
                    r.PatientName.Contains(search) ||
                    r.PatientPhone.Contains(search));

            var history = query
                .OrderByDescending(r => r.ReservedAt)
                .Select(r => new HistoryItemVM
                {
                    ReservationId = r.ReservationId,
                    PatientName = r.PatientName,
                    PatientPhone = r.PatientPhone,
                    BedTypeName = r.BedTypes != null ? r.BedTypes.Name : "—",
                    CaseDescription = r.CaseDescription,
                    Status = r.Status,
                    ETA = r.ETA,
                    ReservedAt = r.ReservedAt
                }).ToList();

            ViewBag.CurrentSearch = search;
            return View(history);
        }


              public IActionResult GetNotifications()
        {
            int hospitalId = GetHospitalId();

            var list = _db.HospitalReservations
                .Include(r => r.BedTypes)
                .Where(r => r.HospitalId == hospitalId && r.Status == "Pending")
                .OrderByDescending(r => r.ReservedAt)
                .Take(10)
                .ToList();

            return Json(new
            {
                count = list.Count,
                notifications = list.Select(r => new
                {
                    r.ReservationId,
                    r.PatientName,
                    BedType = r.BedTypes!.Name,
                    r.ETA
                })
            });
        }

        public IActionResult BlockUser()
        {
            int hospitalId = GetHospitalId();

            var blocked = _db.HospitalBlockedPhones
                .Where(b => b.HospitalId == hospitalId)
                .OrderByDescending(b => b.BlockedAt)
                .Select(b => new BlockedUserVM
                {
                    Id = b.Id,
                    Phone = b.Phone,
                    Reason = b.Reason,
                    BlockedAt = b.BlockedAt
                }).ToList();

            ViewBag.BlockedList = blocked;
            return View(new BlockPhoneVM());
        }

        [HttpPost]
        public IActionResult BlockPhone(BlockPhoneVM vm)
        {
            int hospitalId = GetHospitalId();

            if (!ModelState.IsValid)
            {
                ViewBag.BlockedList = _db.HospitalBlockedPhones
                    .Where(b => b.HospitalId == hospitalId)
                    .OrderByDescending(b => b.BlockedAt)
                    .Select(b => new BlockedUserVM
                    {
                        Id = b.Id,
                        Phone = b.Phone,
                        Reason = b.Reason,
                        BlockedAt = b.BlockedAt
                    }).ToList();
                return View("BlockUser", vm);
            }

            bool alreadyBlocked = _db.HospitalBlockedPhones.Any(b =>
                b.HospitalId == hospitalId && b.Phone == vm.Phone);

            if (alreadyBlocked)
            {
                ModelState.AddModelError("Phone", "This number is already blocked");
                ViewBag.BlockedList = _db.HospitalBlockedPhones
                    .Where(b => b.HospitalId == hospitalId)
                    .OrderByDescending(b => b.BlockedAt)
                    .Select(b => new BlockedUserVM
                    {
                        Id = b.Id,
                        Phone = b.Phone,
                        Reason = b.Reason,
                        BlockedAt = b.BlockedAt
                    }).ToList();
                return View("BlockUser", vm);
            }

            _db.HospitalBlockedPhones.Add(new BlockedPhones
            {
                Phone = vm.Phone,
                HospitalId = hospitalId,
                Reason = vm.Reason,
                BlockedAt = DateTime.Now
            });
            _db.SaveChanges();

            TempData["BlockSuccess"] = $"? {vm.Phone} has been blocked";
            return RedirectToAction("BlockUser");
        }

        [HttpPost]
        public IActionResult UnblockPhone(int id)
        {
            int hospitalId = GetHospitalId();

            var blocked = _db.HospitalBlockedPhones
                .FirstOrDefault(b => b.Id == id && b.HospitalId == hospitalId);

            if (blocked != null)
            {
                _db.HospitalBlockedPhones.Remove(blocked);
                _db.SaveChanges();
            }

            TempData["BlockSuccess"] = "? Number unblocked successfully";
            return RedirectToAction("BlockUser");
        }
    }
}



