using Microsoft.AspNetCore.Mvc;
using Mawidy.Application.Interfaces;
using Mawidy.Application.DTOs;

namespace Mawidy.Controllers;

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
