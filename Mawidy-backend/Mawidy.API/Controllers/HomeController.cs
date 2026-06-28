using Microsoft.AspNetCore.Mvc;

namespace Mawidy.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
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
