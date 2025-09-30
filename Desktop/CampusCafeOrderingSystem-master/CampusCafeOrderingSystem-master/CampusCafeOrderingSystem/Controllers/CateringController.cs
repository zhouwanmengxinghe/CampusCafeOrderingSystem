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

            // Check if application is approved before allowing payment
            if (application.Status != ApplicationStatus.Approved)
            {
                TempData["ErrorMessage"] = "This application must be approved before payment can be processed.";
                return RedirectToAction(nameof(Confirmation), new { id = id });
            }

            var paymentModel = new CampusCafeOrderingSystem.Models.PaymentModel
            {
                TotalAmount = (application.BudgetPerPerson ?? 0) * application.NumberOfPeople
            };

            ViewBag.ApplicationId = id;
            ViewBag.ApplicationDetails = application;
            return View("~/Views/user_order_pay/Payment/Checkout.cshtml", paymentModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCateringPayment(PaymentModel model, int applicationId)
        {
            var application = await _db.CateringApplications
                .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
            {
                TempData["ErrorMessage"] = "Application not found.";
                return RedirectToAction("Apply");
            }

            // Set payment method from selected payment method
            model.PaymentMethod = model.SelectedPaymentMethod;

            // Validate payment
            var validationResult = ValidatePayment(model);
            if (!validationResult.IsValid)
            {
                TempData["ErrorMessage"] = validationResult.ErrorMessage;
                return RedirectToAction(nameof(Payment), new { id = applicationId });
            }

            // Process payment
            var paymentResult = ProcessPaymentMethod(model);
            if (!paymentResult.IsSuccessful)
            {
                TempData["ErrorMessage"] = paymentResult.Message;
                return RedirectToAction(nameof(Payment), new { id = applicationId });
            }

            var totalAmount = (application.BudgetPerPerson ?? 0) * application.NumberOfPeople;
            paymentResult.Amount = totalAmount;
            
            // Update application status to paid
            application.Status = ApplicationStatus.Paid;
            await _db.SaveChangesAsync();

            // Store payment result in TempData
            TempData["PaymentResult"] = System.Text.Json.JsonSerializer.Serialize(paymentResult);

            return RedirectToAction("PaymentConfirmation", new { id = applicationId });
        }

        [HttpGet]
        public IActionResult PaymentConfirmation(int id)
        {
            PaymentResult paymentResult;
            
            // Try to get payment result from TempData
            var paymentResultJson = TempData["PaymentResult"] as string;
            if (!string.IsNullOrEmpty(paymentResultJson))
            {
                try
                {
                    paymentResult = System.Text.Json.JsonSerializer.Deserialize<PaymentResult>(paymentResultJson);
                }
                catch
                {
                    // Fallback if deserialization fails
                    paymentResult = CreateFallbackPaymentResult();
                }
            }
            else
            {
                // Fallback payment result
                paymentResult = CreateFallbackPaymentResult();
            }
            
            return View("~/Views/user_order_pay/Payment/PaymentSuccess.cshtml", paymentResult);
        }

        private PaymentResult CreateFallbackPaymentResult()
        {
            return new PaymentResult
            {
                IsSuccess = true,
                Message = "Catering payment completed successfully!",
                TransactionId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TransactionTime = DateTime.Now,
                Amount = 0,
                PaymentMethod = "Unknown"
            };
        }

        private (bool IsValid, string ErrorMessage) ValidatePayment(PaymentModel model)
        {
            // 简化验证逻辑 - 只要选择了支付方式就通过
            if (string.IsNullOrWhiteSpace(model.SelectedPaymentMethod))
            {
                return (false, "Please select a payment method.");
            }

            return (true, string.Empty);
        }

        private PaymentResult ProcessPaymentMethod(PaymentModel model)
        {
            // 模拟支付处理 - 任何输入都成功
            var message = model.SelectedPaymentMethod switch
            {
                "CampusCard" => "Payment completed successfully using Campus Card!",
                "CreditCard" => "Payment completed successfully using Credit/Debit Card!",
                "MobileWallet" => "Payment completed successfully using Mobile Wallet!",
                _ => "Payment completed successfully!"
            };

            return new PaymentResult
            {
                IsSuccess = true,
                Message = message,
                TransactionId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TransactionTime = DateTime.Now,
                PaymentMethod = model.PaymentMethod
            };
        }
    }
}
