using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class MerchantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult MenuManagement()
        {
            return View();
        }

        public IActionResult OrderManagement()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult BusinessReports()
        {
            return View();
        }

        public IActionResult ReviewManagement()
        {
            return View();
        }
    }
}