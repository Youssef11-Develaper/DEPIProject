using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelecomBranches.Data;
using TelecomBranches.Services;
using TelecomBranches.ViewModels;

namespace TelecomBranches.Controllers;

public class BranchController : Controller
{
    private readonly IBranchService      _branchSvc;
    private readonly IAppointmentService _apptSvc;
    private readonly AppDbContext        _db;

    public BranchController(IBranchService branchSvc, IAppointmentService apptSvc, AppDbContext db)
    {
        _branchSvc = branchSvc;
        _apptSvc   = apptSvc;
        _db        = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(BranchFilterViewModel filter)
    {
        var vm = await _branchSvc.GetBranchListAsync(filter);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var vm = await _branchSvc.GetBranchDetailAsync(id);
        if (vm is null) return NotFound();
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_BranchDetail", vm);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Docs(string serviceKey)
    {
        var docs = await _branchSvc.GetServiceDocumentsAsync(serviceKey);
        return Json(docs);
    }

    [HttpGet]
    public async Task<IActionResult> Districts(int governorateId)
    {
        var districts = await _branchSvc.GetDistrictsByGovernorateAsync(governorateId);
        return Json(districts);
    }

    [HttpGet]
    public async Task<IActionResult> Book(int id, string? serviceKey = null)
    {
        var vm = await _apptSvc.GetBookingFormAsync(id, serviceKey);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(AppointmentCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var fresh = await _apptSvc.GetBookingFormAsync(vm.BranchId, vm.ServiceKey);
            if (fresh is not null) vm.AvailableServices = fresh.AvailableServices;
            return View(vm);
        }
        var confirm = await _apptSvc.CreateAppointmentAsync(vm);
        return RedirectToAction(nameof(Confirm), new { id = confirm.AppointmentId });
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(int id)
    {
        var appt = await _db.Appointments
            .Include(a => a.Branch).ThenInclude(b => b.Operator)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt is null) return NotFound();

        var svcNames = new Dictionary<string, string>
        {
            ["new"]  = "استخراج خط جديد",  ["sim"]   = "استبدال شريحة SIM",
            ["pkg"]  = "تجديد / ترقية باقة",["bill"]  = "دفع الفواتير",
            ["trans"]= "نقل ملكية الخط",    ["comp"]  = "تقديم شكوى",
            ["esim"] = "تفعيل eSIM",         ["roam"]  = "تجوال دولي",
            ["fiber"]= "Fiber / ADSL",       ["land"]  = "خط أرضي"
        };

        return View(new AppointmentConfirmViewModel
        {
            AppointmentId   = appt.Id,
            BranchName      = appt.Branch.NameAr,
            OperatorName    = appt.Branch.Operator.NameAr,
            ServiceName     = svcNames.GetValueOrDefault(appt.ServiceKey, appt.ServiceKey),
            CustomerName    = appt.CustomerName,
            AppointmentDate = appt.AppointmentDate,
            AppointmentTime = appt.AppointmentTime.ToString(@"hh\:mm")
        });
    }

    [HttpGet]
    public async Task<IActionResult> Slots(int branchId, DateTime date)
    {
        var slots = await _apptSvc.GetAvailableSlotsAsync(branchId, date);
        return Json(slots);
    }

    [HttpPost]
    public async Task<IActionResult> JoinQueue([FromBody] JoinQueueViewModel vm)
    {
        var status = await _apptSvc.JoinVirtualQueueAsync(vm);
        return Json(status);
    }

    [HttpPost]
    public async Task<IActionResult> LeaveQueue(int id)
    {
        await _apptSvc.LeaveQueueAsync(id);
        return Json(new { success = true });
    }
}
