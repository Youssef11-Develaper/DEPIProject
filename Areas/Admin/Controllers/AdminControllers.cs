using Microsoft.AspNetCore.Mvc;
using TelecomBranches.Areas.Admin.Models;
using TelecomBranches.Areas.Admin.Services;

namespace TelecomBranches.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    private readonly IAdminService _svc;
    public DashboardController(IAdminService svc) => _svc = svc;

    [HttpGet, Route("/admin"), Route("/admin/dashboard")]
    public async Task<IActionResult> Index()
    {
        var vm = await _svc.GetDashboardAsync();
        return View(vm);
    }
}

[Area("Admin")]
public class BranchesController : Controller
{
    private readonly IAdminService _svc;
    public BranchesController(IAdminService svc) => _svc = svc;

    [HttpGet, Route("/admin/branches")]
    public async Task<IActionResult> Index(AdminBranchListViewModel filter)
    {
        var vm = await _svc.GetBranchesAsync(filter);
        return View(vm);
    }

    [HttpGet, Route("/admin/branches/create")]
    public async Task<IActionResult> Create()
    {
        var vm = await _svc.GetBranchEditAsync();
        return View("Edit", vm);
    }

    [HttpGet, Route("/admin/branches/edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _svc.GetBranchEditAsync(id);
        return View(vm);
    }

    [HttpPost, Route("/admin/branches/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AdminBranchEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var fresh = await _svc.GetBranchEditAsync(vm.Id);
            vm.Operators = fresh.Operators; vm.Governorates = fresh.Governorates; vm.Districts = fresh.Districts;
            return View("Edit", vm);
        }
        await _svc.SaveBranchAsync(vm);
        TempData["Success"] = vm.Id == 0 ? "✅ تم إضافة الفرع" : "✅ تم تعديل الفرع";
        return Redirect("/admin/branches");
    }

    [HttpPost, Route("/admin/branches/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteBranchAsync(id);
        TempData["Success"] = "🗑️ تم حذف الفرع";
        return Redirect("/admin/branches");
    }

    [HttpGet, Route("/admin/branches/getdistricts")]
    public async Task<IActionResult> GetDistricts(int governorateId)
    {
        var list = await _svc.GetDistrictsSelectAsync(governorateId);
        return Json(list);
    }
}

[Area("Admin")]
public class AppointmentsController : Controller
{
    private readonly IAdminService _svc;
    public AppointmentsController(IAdminService svc) => _svc = svc;

    [HttpGet, Route("/admin/appointments")]
    public async Task<IActionResult> Index(AdminAppointmentListViewModel filter)
    {
        var vm = await _svc.GetAppointmentsAsync(filter);
        return View(vm);
    }

    [HttpPost, Route("/admin/appointments/updatestatus")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await _svc.UpdateAppointmentStatusAsync(id, status);
        return Json(new { success = true });
    }
}

[Area("Admin")]
public class ReportsController : Controller
{
    private readonly IAdminService _svc;
    public ReportsController(IAdminService svc) => _svc = svc;

    [HttpGet, Route("/admin/reports")]
    public async Task<IActionResult> Index(string period = "month")
    {
        var vm = await _svc.GetReportsAsync(period);
        return View(vm);
    }
}
