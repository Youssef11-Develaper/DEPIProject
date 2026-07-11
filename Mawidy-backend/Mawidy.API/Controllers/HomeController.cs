using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Mawidy.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/index.html");
        }

        public IActionResult Appointments()
        {
            return View();
        }

        public IActionResult MyAppointments()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Complaints()
        {
            return View();
        }
    }
}
