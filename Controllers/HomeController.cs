using Microsoft.AspNetCore.Mvc;
using TelecomBranches.Services;

namespace TelecomBranches.Controllers;

public class HomeController : Controller
{
    private readonly IBranchService _svc;
    public HomeController(IBranchService svc) => _svc = svc;

    // GET / — صفحة اختيار الشركة
    public async Task<IActionResult> Index()
    {
        var operators = await _svc.GetOperatorsAsync();
        return View(operators);
    }
}
