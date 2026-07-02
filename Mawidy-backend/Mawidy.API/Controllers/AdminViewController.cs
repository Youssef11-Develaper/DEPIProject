using Microsoft.AspNetCore.Mvc;

namespace Mawidy.API.Controllers
{
    [Route("Admin")]
    public class AdminViewController : Controller
    {
        [Route("Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Route("Appointments")]
        public IActionResult Appointments()
        {
            return View();
        }

        [Route("Branches")]
        public IActionResult Branches()
        {
            return View();
        }

        [Route("Complaints")]
        public IActionResult Complaints()
        {
            return View();
        }

        [Route("Services")]
        public IActionResult Services()
        {
            return View();
        }

        [Route("Users")]
        public IActionResult Users()
        {
            return View();
        }
    }
}
