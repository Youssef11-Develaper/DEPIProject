using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TelecomBranches.Areas.Admin.Models;
using TelecomBranches.Data;
using TelecomBranches.Models;

namespace TelecomBranches.Areas.Admin.Services;

public interface IAdminService
{
    Task<AdminDashboardViewModel>    GetDashboardAsync();
    Task<AdminBranchListViewModel>   GetBranchesAsync(AdminBranchListViewModel filter);
    Task<AdminBranchEditViewModel>   GetBranchEditAsync(int id = 0);
    Task<int>                        SaveBranchAsync(AdminBranchEditViewModel vm);
    Task                             DeleteBranchAsync(int id);
    Task<AdminAppointmentListViewModel> GetAppointmentsAsync(AdminAppointmentListViewModel filter);
    Task                             UpdateAppointmentStatusAsync(int id, string status);
    Task<AdminReportsViewModel>      GetReportsAsync(string period);
    Task<List<SelectListItem>>       GetDistrictsSelectAsync(int governorateId);
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    private static readonly Dictionary<string, (string Name, string Icon)> SvcInfo = new()
    {
        ["new"]   = ("استخراج خط جديد", "📲"),
        ["sim"]   = ("استبدال SIM",      "🔄"),
        ["pkg"]   = ("تجديد باقة",       "📶"),
        ["bill"]  = ("دفع فواتير",       "💸"),
        ["trans"] = ("نقل ملكية",        "🔁"),
        ["comp"]  = ("شكوى",             "📢"),
        ["esim"]  = ("تفعيل eSIM",       "💡"),
        ["roam"]  = ("تجوال دولي",       "🌍"),
        ["fiber"] = ("Fiber/ADSL",       "🌐"),
        ["land"]  = ("خط أرضي",          "☎️"),
    };

    public AdminService(AppDbContext db) => _db = db;

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.Today;

        var branches = await _db.Branches
            .Include(b => b.Operator)
            .Include(b => b.Governorate)
            .Include(b => b.District)
            .ToListAsync();

        var appointments = await _db.Appointments
            .Include(a => a.Branch).ThenInclude(b => b.Operator)
            .Include(a => a.Branch).ThenInclude(b => b.Governorate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var queueCount = await _db.VirtualQueueEntries.CountAsync(q => q.IsActive);
        var operators  = await _db.Operators.ToListAsync();
        var govs       = await _db.Governorates
            .Include(g => g.Districts).ThenInclude(d => d.Branches)
            .OrderByDescending(g => g.Districts.SelectMany(d => d.Branches).Count())
            .Take(10).ToListAsync();

        // Operator stats
        var opStats = operators.Select(op =>
        {
            var ob = branches.Where(b => b.OperatorId == op.Id).ToList();
            double avg = ob.Any() ? Math.Round(ob.Average(b => b.Rating), 1) : 0;
            return new AdminOperatorStat
            {
                OperatorKey      = op.Key,
                NameAr           = op.NameAr,
                Color            = op.Color,
                BgColor          = op.BgColor,
                Emoji            = op.Emoji,
                BranchCount      = ob.Count,
                OpenCount        = ob.Count(b => b.Status == BranchStatus.Open),
                AppointmentCount = appointments.Count(a => a.Branch.OperatorId == op.Id),
                AvgRating        = avg
            };
        }).ToList();

        // Gov stats
        var govStats = govs.Select(g => new AdminGovStat
        {
            Id               = g.Id,
            NameAr           = g.NameAr,
            Emoji            = g.Emoji,
            Region           = g.Region,
            BranchCount      = g.Districts.SelectMany(d => d.Branches).Count(),
            DistrictCount    = g.Districts.Count,
            AppointmentCount = appointments.Count(a => a.Branch.GovernorateId == g.Id)
        }).ToList();

        // Service stats
        int totalA = Math.Max(1, appointments.Count);
        var svcStats = appointments
            .GroupBy(a => a.ServiceKey)
            .Select(grp =>
            {
                var inf = SvcInfo.GetValueOrDefault(grp.Key, (Name: grp.Key, Icon: "🔧"));
                return new AdminServiceStat
                {
                    ServiceKey = grp.Key,
                    NameAr     = inf.Name,
                    Icon       = inf.Icon,
                    Count      = grp.Count(),
                    Percentage = Math.Round(grp.Count() * 100.0 / totalA, 1)
                };
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // Weekly stats
        string[] dayNames = { "الأحد","الاثنين","الثلاثاء","الأربعاء","الخميس","الجمعة","السبت" };
        var last30 = appointments.Where(a => a.AppointmentDate >= today.AddDays(-30)).ToList();
        var dayCounts = Enumerable.Range(0, 7)
            .Select(d => last30.Count(a => (int)a.AppointmentDate.DayOfWeek == d))
            .ToList();
        int maxDay = dayCounts.DefaultIfEmpty(1).Max();
        var weeklyStats = dayNames.Select((n, i) =>
            new AdminDayStat { DayAr = n, Count = dayCounts[i], MaxCount = maxDay }).ToList();

        // Busiest branches
        var busiest = branches
            .Where(b => b.Status != BranchStatus.Closed)
            .OrderByDescending(b => b.QueueCount)
            .Take(8)
            .Select(b => new AdminBusyBranch
            {
                Id            = b.Id,
                NameAr        = b.NameAr,
                OperatorKey   = b.Operator.Key,
                OperatorEmoji = b.Operator.Emoji,
                OperatorColor = b.Operator.Color,
                OperatorBg    = b.Operator.BgColor,
                GovName       = b.Governorate.NameAr,
                DistName      = b.District.NameAr,
                QueueCount    = b.QueueCount,
                WaitTime      = b.WaitTime,
                Status        = b.Status.ToString().ToLower()
            }).ToList();

        return new AdminDashboardViewModel
        {
            TotalBranches       = branches.Count,
            OpenBranches        = branches.Count(b => b.Status == BranchStatus.Open),
            BusyBranches        = branches.Count(b => b.Status == BranchStatus.Busy),
            ClosedBranches      = branches.Count(b => b.Status == BranchStatus.Closed),
            TotalAppointments   = appointments.Count,
            TodayAppointments   = appointments.Count(a => a.AppointmentDate == today),
            PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
            TotalGovernorates   = await _db.Governorates.CountAsync(),
            TotalDistricts      = await _db.Districts.CountAsync(),
            ActiveQueueEntries  = queueCount,
            OperatorStats       = opStats,
            TopGovernorates     = govStats,
            ServiceStats        = svcStats,
            WeeklyStats         = weeklyStats,
            RecentAppointments  = appointments.Take(10).Select(MapAppointment).ToList(),
            BusiestBranches     = busiest
        };
    }

    // ── Branches ──────────────────────────────────────────────────────────────
    public async Task<AdminBranchListViewModel> GetBranchesAsync(AdminBranchListViewModel filter)
    {
        var q = _db.Branches
            .Include(b => b.Operator)
            .Include(b => b.Governorate)
            .Include(b => b.District)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.SearchQuery))
        {
            var s = filter.SearchQuery.Trim();
            q = q.Where(b => b.NameAr.Contains(s) || b.Address.Contains(s) ||
                              b.Governorate.NameAr.Contains(s) || b.District.NameAr.Contains(s));
        }
        if (!string.IsNullOrEmpty(filter.FilterOp))
            q = q.Where(b => b.Operator.Key == filter.FilterOp);
        if (filter.FilterGov.HasValue)
            q = q.Where(b => b.GovernorateId == filter.FilterGov.Value);
        if (!string.IsNullOrEmpty(filter.FilterStatus))
        {
            int st = filter.FilterStatus switch
            {
                "open"   => (int)BranchStatus.Open,
                "busy"   => (int)BranchStatus.Busy,
                "closed" => (int)BranchStatus.Closed,
                _        => -1
            };
            if (st >= 0) q = q.Where(b => (int)b.Status == st);
        }

        var total = await q.CountAsync();
        int page  = Math.Max(1, filter.Page);
        var items = await q.OrderBy(b => b.GovernorateId).ThenBy(b => b.NameAr)
            .Skip((page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        var apptCounts = await _db.Appointments
            .GroupBy(a => a.BranchId)
            .Select(g => new { BranchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BranchId, x => x.Count);

        return new AdminBranchListViewModel
        {
            Branches = items.Select(b => new AdminBranchRow
            {
                Id               = b.Id,
                NameAr           = b.NameAr,
                OperatorKey      = b.Operator.Key,
                OperatorName     = b.Operator.NameAr,
                OperatorEmoji    = b.Operator.Emoji,
                OperatorColor    = b.Operator.Color,
                OperatorBg       = b.Operator.BgColor,
                GovName          = b.Governorate.NameAr,
                DistName         = b.District.NameAr,
                Address          = b.Address,
                Status           = b.Status.ToString().ToLower(),
                StatusLabel      = b.Status switch { BranchStatus.Open => "متاح", BranchStatus.Busy => "مزدحم", _ => "مغلق" },
                QueueCount       = b.QueueCount,
                WaitTime         = b.WaitTime,
                Rating           = b.Rating,
                AppointmentCount = apptCounts.GetValueOrDefault(b.Id, 0)
            }).ToList(),
            TotalCount   = total,
            Page         = page,
            PageSize     = filter.PageSize,
            SearchQuery  = filter.SearchQuery,
            FilterOp     = filter.FilterOp,
            FilterGov    = filter.FilterGov,
            FilterStatus = filter.FilterStatus,
            Operators    = await GetOperatorsSelectAsync(),
            Governorates = await GetGovernoratesSelectAsync()
        };
    }

    public async Task<AdminBranchEditViewModel> GetBranchEditAsync(int id = 0)
    {
        var vm = new AdminBranchEditViewModel
        {
            Operators    = await GetOperatorsSelectAsync(),
            Governorates = await GetGovernoratesSelectAsync()
        };
        if (id > 0)
        {
            var b = await _db.Branches.FindAsync(id);
            if (b is not null)
            {
                vm.Id = b.Id; vm.OperatorId = b.OperatorId; vm.GovernorateId = b.GovernorateId;
                vm.DistrictId = b.DistrictId; vm.NameAr = b.NameAr; vm.Area = b.Area;
                vm.Address = b.Address; vm.DistanceKm = b.DistanceKm; vm.Status = (int)b.Status;
                vm.QueueCount = b.QueueCount; vm.WaitTime = b.WaitTime; vm.Rating = b.Rating;
                vm.Districts = await GetDistrictsSelectAsync(b.GovernorateId);
            }
        }
        return vm;
    }

    public async Task<int> SaveBranchAsync(AdminBranchEditViewModel vm)
    {
        Branch branch;
        if (vm.Id > 0)
        {
            branch = (await _db.Branches.FindAsync(vm.Id))!;
        }
        else
        {
            branch = new Branch();
            _db.Branches.Add(branch);
        }
        branch.OperatorId    = vm.OperatorId;
        branch.GovernorateId = vm.GovernorateId;
        branch.DistrictId    = vm.DistrictId;
        branch.NameAr        = vm.NameAr;
        branch.Area          = vm.Area;
        branch.Address       = vm.Address;
        branch.DistanceKm    = vm.DistanceKm;
        branch.Status        = (BranchStatus)vm.Status;
        branch.QueueCount    = vm.QueueCount;
        branch.WaitTime      = vm.WaitTime;
        branch.Rating        = vm.Rating;
        await _db.SaveChangesAsync();
        return branch.Id;
    }

    public async Task DeleteBranchAsync(int id)
    {
        var b = await _db.Branches.FindAsync(id);
        if (b is not null) { _db.Branches.Remove(b); await _db.SaveChangesAsync(); }
    }

    // ── Appointments ──────────────────────────────────────────────────────────
    public async Task<AdminAppointmentListViewModel> GetAppointmentsAsync(AdminAppointmentListViewModel filter)
    {
        var q = _db.Appointments
            .Include(a => a.Branch).ThenInclude(b => b.Operator)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.FilterStatus))
        {
            var st = filter.FilterStatus switch
            {
                "confirmed" => AppointmentStatus.Confirmed,
                "pending"   => AppointmentStatus.Pending,
                "cancelled" => AppointmentStatus.Cancelled,
                _           => AppointmentStatus.Completed
            };
            q = q.Where(a => a.Status == st);
        }
        if (!string.IsNullOrEmpty(filter.FilterOp))
            q = q.Where(a => a.Branch.Operator.Key == filter.FilterOp);
        if (!string.IsNullOrEmpty(filter.FilterDate) && DateTime.TryParse(filter.FilterDate, out var dt))
            q = q.Where(a => a.AppointmentDate == dt.Date);

        var total = await q.CountAsync();
        int page  = Math.Max(1, filter.Page);
        var items = await q.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return new AdminAppointmentListViewModel
        {
            Appointments = items.Select(MapAppointment).ToList(),
            TotalCount   = total,
            Page         = page,
            PageSize     = filter.PageSize,
            FilterStatus = filter.FilterStatus,
            FilterOp     = filter.FilterOp,
            FilterDate   = filter.FilterDate,
            Operators    = await GetOperatorsSelectAsync()
        };
    }

    public async Task UpdateAppointmentStatusAsync(int id, string status)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a is null) return;
        a.Status = status switch
        {
            "confirmed" => AppointmentStatus.Confirmed,
            "cancelled" => AppointmentStatus.Cancelled,
            "completed" => AppointmentStatus.Completed,
            _           => AppointmentStatus.Pending
        };
        await _db.SaveChangesAsync();
    }

    // ── Reports ───────────────────────────────────────────────────────────────
    public async Task<AdminReportsViewModel> GetReportsAsync(string period)
    {
        var from = period switch
        {
            "week" => DateTime.Today.AddDays(-7),
            "year" => DateTime.Today.AddYears(-1),
            _      => DateTime.Today.AddMonths(-1)
        };

        var appts = await _db.Appointments
            .Include(a => a.Branch).ThenInclude(b => b.Operator)
            .Include(a => a.Branch).ThenInclude(b => b.Governorate)
            .Where(a => a.CreatedAt >= from)
            .ToListAsync();

        var branches  = await _db.Branches.Include(b => b.Operator).ToListAsync();
        var operators = await _db.Operators.ToListAsync();

        var opStats = operators.Select(op =>
        {
            var ob  = branches.Where(b => b.OperatorId == op.Id).ToList();
            double avg = ob.Any() ? Math.Round(ob.Average(b => b.Rating), 1) : 0;
            return new AdminOperatorStat
            {
                OperatorKey = op.Key, NameAr = op.NameAr, Color = op.Color,
                BgColor = op.BgColor, Emoji = op.Emoji,
                BranchCount      = ob.Count,
                OpenCount        = ob.Count(b => b.Status == BranchStatus.Open),
                AppointmentCount = appts.Count(a => a.Branch.OperatorId == op.Id),
                AvgRating        = avg
            };
        }).ToList();

        var govStats = await _db.Governorates
            .Include(g => g.Districts).ThenInclude(d => d.Branches)
            .OrderByDescending(g => g.Districts.SelectMany(d => d.Branches).Count())
            .Take(15)
            .Select(g => new AdminGovStat
            {
                Id           = g.Id,
                NameAr       = g.NameAr,
                Emoji        = g.Emoji,
                Region       = g.Region,
                BranchCount  = g.Districts.SelectMany(d => d.Branches).Count(),
                DistrictCount= g.Districts.Count
            }).ToListAsync();

        foreach (var gs in govStats)
            gs.AppointmentCount = appts.Count(a => a.Branch.GovernorateId == gs.Id);

        int tot = Math.Max(1, appts.Count);
        var svcStats = appts.GroupBy(a => a.ServiceKey)
            .Select(grp =>
            {
                var inf = SvcInfo.GetValueOrDefault(grp.Key, (Name: grp.Key, Icon: "🔧"));
                return new AdminServiceStat
                {
                    ServiceKey = grp.Key, NameAr = inf.Name, Icon = inf.Icon,
                    Count = grp.Count(), Percentage = Math.Round(grp.Count() * 100.0 / tot, 1)
                };
            })
            .OrderByDescending(s => s.Count).ToList();

        var monthly = Enumerable.Range(0, 12).Select(i =>
        {
            var m = DateTime.Today.AddMonths(-11 + i);
            return new AdminMonthlyStat
            {
                MonthLabel = m.ToString("MMM yy"),
                Count      = appts.Count(a => a.AppointmentDate.Year == m.Year && a.AppointmentDate.Month == m.Month)
            };
        }).ToList();

        return new AdminReportsViewModel
        {
            Period        = period,
            TotalAppts    = appts.Count,
            TotalBranches = branches.Count,
            OperatorStats = opStats,
            GovStats      = govStats,
            ServiceStats  = svcStats,
            MonthlyTrend  = monthly
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    public async Task<List<SelectListItem>> GetDistrictsSelectAsync(int governorateId) =>
        await _db.Districts.Where(d => d.GovernorateId == governorateId).OrderBy(d => d.NameAr)
            .Select(d => new SelectListItem(d.NameAr, d.Id.ToString())).ToListAsync();

    private async Task<List<SelectListItem>> GetOperatorsSelectAsync() =>
        await _db.Operators
            .Select(o => new SelectListItem(o.NameAr + " " + o.Emoji, o.Id.ToString()))
            .ToListAsync();

    private async Task<List<SelectListItem>> GetGovernoratesSelectAsync() =>
        await _db.Governorates.OrderBy(g => g.SortOrder)
            .Select(g => new SelectListItem(g.Emoji + " " + g.NameAr, g.Id.ToString()))
            .ToListAsync();

    private static AdminAppointmentRow MapAppointment(Appointment a)
    {
        var inf = SvcInfo.GetValueOrDefault(a.ServiceKey, (Name: a.ServiceKey, Icon: "🔧"));
        var (stLabel, stClass) = a.Status switch
        {
            AppointmentStatus.Confirmed => ("مؤكد",   "confirmed"),
            AppointmentStatus.Pending   => ("انتظار", "pending"),
            AppointmentStatus.Cancelled => ("ملغي",   "cancelled"),
            _                           => ("مكتمل",  "completed")
        };
        return new AdminAppointmentRow
        {
            Id            = a.Id,
            CustomerName  = a.CustomerName,
            CustomerPhone = a.CustomerPhone,
            BranchName    = a.Branch?.NameAr ?? "-",
            OperatorKey   = a.Branch?.Operator?.Key ?? "",
            OperatorEmoji = a.Branch?.Operator?.Emoji ?? "",
            OperatorColor = a.Branch?.Operator?.Color ?? "",
            OperatorBg    = a.Branch?.Operator?.BgColor ?? "",
            ServiceIcon   = inf.Icon,
            ServiceName   = inf.Name,
            Date          = a.AppointmentDate,
            Time          = a.AppointmentTime.ToString(@"hh\:mm"),
            StatusLabel   = stLabel,
            StatusClass   = stClass
        };
    }
}
