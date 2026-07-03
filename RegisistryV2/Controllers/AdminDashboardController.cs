using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;
using RegisistryV2.Services;
using RegisistryV2.ViewModel.Admin;

namespace RegisistryV2.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminDashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly PdfReportService _pdfReportService;
        private readonly IBranchRepository _branchRepository;
        private readonly IComplaintRepository _complaintRepository;

        public AdminDashboardController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            EmailService emailService,
            PdfReportService pdfReportService,
            IBranchRepository branchRepository,
            IComplaintRepository complaintRepository)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _pdfReportService = pdfReportService;
            _branchRepository = branchRepository;
            _complaintRepository = complaintRepository;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var allAppointments = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Branch)
                    .ThenInclude(b => b.Governorate)
                .ToListAsync();

            var totalAppointments = allAppointments.Count;

            var viewModel = new DashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalBranches = await _context.Branches.CountAsync(),
                TotalAppointments = totalAppointments,
                TotalComplaints = await _context.Complaints.CountAsync(),

                TodayAppointments = allAppointments
                    .Count(a => a.AppointmentDate.Date == today),

                TodayCompleted = allAppointments
                    .Count(a => a.AppointmentDate.Date == today
                        && a.Status == AppointmentStatus.Completed),

                TodayCancelled = allAppointments
                    .Count(a => a.AppointmentDate.Date == today
                        && a.Status == AppointmentStatus.Cancelled),

                PendingComplaints = await _context.Complaints
                    .CountAsync(c => c.Status == ComplaintStatus.Submitted
                        || c.Status == ComplaintStatus.UnderReview),

                TopBranches = await _context.Branches
                    .Select(b => new BranchStatsViewModel
                    {
                        BranchName = b.Name,
                        TotalAppointments = b.Appointments.Count,
                        AverageRating = b.Appointments
                            .SelectMany(a => _context.Ratings.Where(r => r.BranchId == b.Id))
                            .Average(r => (double?)r.Stars) ?? 0
                    })
                    .OrderByDescending(b => b.TotalAppointments)
                    .Take(5)
                    .ToListAsync(),

                WeeklyAppointments = await _context.Appointments
                    .Where(a => a.AppointmentDate.Date >= today.AddDays(-7))
                    .GroupBy(a => a.AppointmentDate.Date)
                    .Select(g => new DailyAppointmentsViewModel
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync(),

                RecentComplaints = await _context.Complaints
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                TopServices = allAppointments
                    .GroupBy(a => a.ServiceType.Name)
                    .Select(g => new ServiceStatsViewModel
                    {
                        ServiceName = g.Key,
                        TotalAppointments = g.Count(),
                        Percentage = totalAppointments > 0
                            ? Math.Round((double)g.Count() / totalAppointments * 100, 1)
                            : 0
                    })
                    .OrderByDescending(s => s.TotalAppointments)
                    .Take(5)
                    .ToList(),

                GovernorateDistribution = allAppointments
                    .GroupBy(a => a.Branch.Governorate.Name)
                    .Select(g => new GovernorateStatsViewModel
                    {
                        GovernorateName = g.Key,
                        TotalAppointments = g.Count()
                    })
                    .OrderByDescending(g => g.TotalAppointments)
                    .ToList(),

                CancellationRate = totalAppointments > 0
                    ? Math.Round((double)allAppointments
                        .Count(a => a.Status == AppointmentStatus.Cancelled)
                        / totalAppointments * 100, 1)
                    : 0
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Appointments(DateTime? date, int? branchId)
        {
            var selectedDate = date ?? DateTime.Today;

            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Branch)
                .Include(a => a.ServiceType)
                .AsQueryable();

            query = query.Where(a => a.AppointmentDate.Date == selectedDate.Date);

            if (branchId.HasValue)
                query = query.Where(a => a.BranchId == branchId);

            var appointments = await query
                .OrderBy(a => a.TimeSlot)
                .ToListAsync();

            ViewBag.Branches = await _context.Branches.ToListAsync();
            ViewBag.SelectedDate = selectedDate;
            ViewBag.SelectedBranch = branchId;

            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث حالة الموعد";
            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> Branches()
        {
            var branches = await _branchRepository.GetAllWithDetailsAsync();
            return View(branches);
        }

        public async Task<IActionResult> CreateBranch()
        {
            ViewBag.Governorates = await _context.Governorates.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBranch(BranchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Governorates = await _context.Governorates.ToListAsync();
                return View(model);
            }

            var branch = new Branch
            {
                Name = model.Name,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                GovernorateId = model.GovernorateId
            };

            await _branchRepository.AddAsync(branch);
            await _branchRepository.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الفرع";
            return RedirectToAction("Branches");
        }

        public async Task<IActionResult> EditBranch(int id)
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null) return NotFound();

            var model = new BranchViewModel
            {
                Name = branch.Name,
                Address = branch.Address,
                Latitude = branch.Latitude,
                Longitude = branch.Longitude,
                GovernorateId = branch.GovernorateId
            };

            ViewBag.BranchId = id;
            ViewBag.Governorates = await _context.Governorates.ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditBranch(int id, BranchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BranchId = id;
                ViewBag.Governorates = await _context.Governorates.ToListAsync();
                return View(model);
            }

            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null) return NotFound();

            branch.Name = model.Name;
            branch.Address = model.Address;
            branch.Latitude = model.Latitude;
            branch.Longitude = model.Longitude;
            branch.GovernorateId = model.GovernorateId;

            _branchRepository.Update(branch);
            await _branchRepository.SaveChangesAsync();

            TempData["Success"] = "تم تعديل الفرع";
            return RedirectToAction("Branches");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null) return NotFound();

            _branchRepository.Delete(branch);
            await _branchRepository.SaveChangesAsync();

            TempData["Success"] = "تم حذف الفرع";
            return RedirectToAction("Branches");
        }

        public async Task<IActionResult> BranchSchedule(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            var schedules = await _context.BranchSchedules
                .Where(s => s.BranchId == id)
                .OrderBy(s => s.DayOfWeek)
                .ToListAsync();

            var holidays = await _context.BranchHolidays
                .Where(h => h.BranchId == id && h.Date >= DateTime.Today)
                .OrderBy(h => h.Date)
                .ToListAsync();

            var unavailabilities = await _context.ServiceUnavailabilities
                .Include(s => s.ServiceType)
                .Where(s => s.BranchId == id && s.Date >= DateTime.Today)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var services = await _context.ServiceTypes.ToListAsync();

            ViewBag.Branch = branch;
            ViewBag.Schedules = schedules;
            ViewBag.Holidays = holidays;
            ViewBag.Unavailabilities = unavailabilities;
            ViewBag.Services = services;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSchedule(int id, TimeSpan openTime, TimeSpan closeTime,
            TimeSpan peakStart, TimeSpan peakEnd, int maxSlot)
        {
            var schedule = await _context.BranchSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            schedule.OpenTime = openTime;
            schedule.CloseTime = closeTime;
            schedule.PeakStartTime = peakStart;
            schedule.PeakEndTime = peakEnd;
            schedule.MaxAppointmentsPerSlot = maxSlot;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث المواعيد";
            return RedirectToAction("BranchSchedule", new { id = schedule.BranchId });
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule(int branchId, DayOfWeek dayOfWeek,
            TimeSpan openTime, TimeSpan closeTime, TimeSpan peakStart,
            TimeSpan peakEnd, int maxSlot)
        {
            var exists = await _context.BranchSchedules
                .AnyAsync(s => s.BranchId == branchId && s.DayOfWeek == dayOfWeek);

            if (exists)
            {
                TempData["Error"] = "اليوم ده موجود بالفعل";
                return RedirectToAction("BranchSchedule", new { id = branchId });
            }

            _context.BranchSchedules.Add(new BranchSchedule
            {
                BranchId = branchId,
                DayOfWeek = dayOfWeek,
                OpenTime = openTime,
                CloseTime = closeTime,
                PeakStartTime = peakStart,
                PeakEndTime = peakEnd,
                MaxAppointmentsPerSlot = maxSlot
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إضافة يوم الشغل";
            return RedirectToAction("BranchSchedule", new { id = branchId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.BranchSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            var branchId = schedule.BranchId;
            _context.BranchSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف يوم الشغل";
            return RedirectToAction("BranchSchedule", new { id = branchId });
        }

        [HttpPost]
        public async Task<IActionResult> AddHoliday(int branchId, DateTime date, string reason)
        {
            var exists = await _context.BranchHolidays
                .AnyAsync(h => h.BranchId == branchId && h.Date.Date == date.Date);

            if (exists)
            {
                TempData["Error"] = "الإجازة دي موجودة بالفعل";
                return RedirectToAction("BranchSchedule", new { id = branchId });
            }

            _context.BranchHolidays.Add(new BranchHoliday
            {
                BranchId = branchId,
                Date = date,
                Reason = reason
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إضافة الإجازة";
            return RedirectToAction("BranchSchedule", new { id = branchId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var holiday = await _context.BranchHolidays.FindAsync(id);
            if (holiday == null) return NotFound();

            var branchId = holiday.BranchId;
            _context.BranchHolidays.Remove(holiday);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الإجازة";
            return RedirectToAction("BranchSchedule", new { id = branchId });
        }

        [HttpPost]
        public async Task<IActionResult> AddServiceUnavailability(int branchId,
            int serviceTypeId, DateTime date, string reason)
        {
            var exists = await _context.ServiceUnavailabilities
                .AnyAsync(s => s.BranchId == branchId
                    && s.ServiceTypeId == serviceTypeId
                    && s.Date.Date == date.Date);

            if (exists)
            {
                TempData["Error"] = "الخدمة دي معطلة بالفعل في اليوم ده";
                return RedirectToAction("BranchSchedule", new { id = branchId });
            }

            _context.ServiceUnavailabilities.Add(new ServiceUnavailability
            {
                BranchId = branchId,
                ServiceTypeId = serviceTypeId,
                Date = date,
                Reason = reason
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تعطيل الخدمة";
            return RedirectToAction("BranchSchedule", new { id = branchId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteServiceUnavailability(int id)
        {
            var item = await _context.ServiceUnavailabilities.FindAsync(id);
            if (item == null) return NotFound();

            var branchId = item.BranchId;
            _context.ServiceUnavailabilities.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إلغاء تعطيل الخدمة";
            return RedirectToAction("BranchSchedule", new { id = branchId });
        }

        public async Task<IActionResult> Services()
        {
            var services = await _context.ServiceTypes
                .Include(s => s.Appointments)
                .ToListAsync();
            return View(services);
        }

        public IActionResult CreateService() => View();

        [HttpPost]
        public async Task<IActionResult> CreateService(ServiceTypeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var service = new ServiceType
            {
                Name = model.Name,
                Description = model.Description,
                DurationMinutes = model.DurationMinutes,
                RequiredDocuments = model.RequiredDocuments
            };

            _context.ServiceTypes.Add(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الخدمة";
            return RedirectToAction("Services");
        }

        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.ServiceTypes.FindAsync(id);
            if (service == null) return NotFound();

            var model = new ServiceTypeViewModel
            {
                Name = service.Name,
                Description = service.Description,
                DurationMinutes = service.DurationMinutes,
                RequiredDocuments = service.RequiredDocuments
            };

            ViewBag.ServiceId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditService(int id, ServiceTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ServiceId = id;
                return View(model);
            }

            var service = await _context.ServiceTypes.FindAsync(id);
            if (service == null) return NotFound();

            service.Name = model.Name;
            service.Description = model.Description;
            service.DurationMinutes = model.DurationMinutes;
            service.RequiredDocuments = model.RequiredDocuments;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تعديل الخدمة";
            return RedirectToAction("Services");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.ServiceTypes.FindAsync(id);
            if (service == null) return NotFound();

            _context.ServiceTypes.Remove(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الخدمة";
            return RedirectToAction("Services");
        }

        public async Task<IActionResult> Users()
        {
            var allUsers = await _userManager.Users
                .Include(u => u.Branch)
                .ToListAsync();

            var branchAdmins = new List<UserWithRoleViewModel>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(Roles.BranchAdmin))
                {
                    branchAdmins.Add(new UserWithRoleViewModel
                    {
                        User = user,
                        Role = Roles.BranchAdmin
                    });
                }
            }

            ViewBag.Branches = await _context.Branches.ToListAsync();
            ViewBag.BranchAdmins = branchAdmins;

            return View(branchAdmins);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string email, string newRole, int? branchId)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                TempData["Error"] = "مفيش حساب بالإيميل ده";
                return RedirectToAction("Users");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            if (newRole == Roles.BranchAdmin && branchId.HasValue)
                user.BranchId = branchId;
            else
                user.BranchId = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"تم تغيير صلاحية {user.FullName} بنجاح";
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> TestEmail()
        {
            var user = await _userManager.GetUserAsync(User);

            try
            {
                await _emailService.SendAsync(
                    user.Email,
                    user.FullName,
                    "تجربة إيميل - السجل المدني",
                    "<h1 style='font-family:Cairo;direction:rtl'>الإيميل شغال تمام! ✅</h1>"
                );
                return Content("تم إرسال الإيميل بنجاح، افتح بريدك الإلكتروني");
            }
            catch (Exception ex)
            {
                return Content("فشل الإرسال: " + ex.Message);
            }
        }

        public async Task<IActionResult> DownloadDailyReport(DateTime? date, int? branchId)
        {
            var selectedDate = date ?? DateTime.Today;
            var pdfBytes = await _pdfReportService.GenerateDailyReportAsync(selectedDate, branchId);
            var fileName = $"تقرير_{selectedDate:yyyy-MM-dd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}  

