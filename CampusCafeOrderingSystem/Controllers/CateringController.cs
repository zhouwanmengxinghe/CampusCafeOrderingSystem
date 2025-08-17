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

        // Admin view catering application list
        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList()
        {
            var list = await _db.CateringApplications
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int id)
        {
            var application = await _db.CateringApplications
                .FirstOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            var paymentModel = new CafeApp.Models.user_order_pay.PaymentModel
            {
                TotalAmount = (application.BudgetPerPerson ?? 0) * application.NumberOfPeople
            };

            ViewBag.ApplicationId = id;
            ViewBag.ApplicationDetails = application;
            return View("~/Views/user_order_pay/Payment/Checkout.cshtml", paymentModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCateringPayment(int applicationId, string paymentMethod)
        {
            var application = await _db.CateringApplications
                .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
            {
                return NotFound();
            }

            var totalAmount = (application.BudgetPerPerson ?? 0) * application.NumberOfPeople;
            
            if (paymentMethod == "CampusCard")
            {
                TempData["Message"] = "Catering payment completed successfully using Campus Card!";
            }
            else if (paymentMethod == "CreditCard")
            {
                TempData["Message"] = "Catering payment completed successfully using Credit/Debit Card!";
            }
            else
            {
                TempData["Message"] = "Invalid payment method selected.";
            }

            TempData["PaymentMethod"] = paymentMethod;
            TempData["TotalAmount"] = totalAmount;
            
            // Update application status to paid (you might want to add a Paid status)
            // application.Status = ApplicationStatus.Paid;
            // await _db.SaveChangesAsync();

            return RedirectToAction("PaymentConfirmation", new { id = applicationId });
        }

        [HttpGet]
        public IActionResult PaymentConfirmation(int id)
        {
            var message = TempData["Message"] as string ?? "Catering payment completed successfully!";
            var paymentMethod = TempData["PaymentMethod"] as string ?? "Unknown";
            var totalAmount = TempData["TotalAmount"] as decimal? ?? 0;
            
            var paymentResult = new CafeApp.Models.user_order_pay.PaymentResult
            {
                IsSuccess = true,
                Message = message,
                TransactionId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TransactionTime = DateTime.Now,
                Amount = totalAmount,
                PaymentMethod = paymentMethod
            };
            
            return View("~/Views/user_order_pay/Payment/PaymentSuccess.cshtml", paymentResult);
        }
    }
}
