using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class LiangController : Controller
    {
        public IActionResult Merchant()
        {
            return View();
        }
    }
}