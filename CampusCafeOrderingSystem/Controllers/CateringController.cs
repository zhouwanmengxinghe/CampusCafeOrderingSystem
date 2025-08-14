using System;
using System.Linq;
using System.Threading.Tasks;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCafeOrderingSystem.Controllers
{
    public class CateringController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CateringController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Apply()
        {
            var vm = new CateringApplicationViewModel
            {
                NumberOfPeople = 10,
                ServiceType = ServiceType.Delivery
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(CateringApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = new CateringApplication
            {
                ContactName = model.ContactName,
                ContactPhone = model.ContactPhone,
                NumberOfPeople = model.NumberOfPeople,
                EventDate = model.EventDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                ServiceType = model.ServiceType,
                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                DietaryCsv = (model.Dietary != null && model.Dietary.Any())
                                ? string.Join(",", model.Dietary)
                                : null,
                BudgetPerPerson = model.BudgetPerPerson,
                MenuStyle = model.MenuStyle,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Pending
            };

            _db.CateringApplications.Add(entity);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your group catering application has been submitted successfully!";
            return RedirectToAction(nameof(Confirmation), new { id = entity.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var data = await _db.CateringApplications
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.ContactName,
                    x.ContactPhone,
                    x.NumberOfPeople,
                    x.EventDate,
                    x.ServiceType,
                    x.Address,
                    x.DietaryCsv,
                    x.BudgetPerPerson,
                    x.MenuStyle,
                    x.Notes,
                    x.CreatedAt,
                    x.Status
                })
                .FirstOrDefaultAsync();

            if (data == null) return NotFound();

            ViewBag.Data = data;
            return View();
        }

        // （可选）管理员查看列表
        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList()
        {
            var list = await _db.CateringApplications
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return View(list);
        }
    }
}
