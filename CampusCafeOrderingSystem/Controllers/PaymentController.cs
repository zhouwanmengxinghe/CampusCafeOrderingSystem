using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using System.Text.Json;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PaymentSuccess(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            // Get payment result from TempData
            var paymentResultJson = TempData["PaymentResult"] as string;
            PaymentResult paymentResult;

            if (!string.IsNullOrEmpty(paymentResultJson))
            {
                paymentResult = JsonSerializer.Deserialize<PaymentResult>(paymentResultJson) ?? new PaymentResult();
            }
            else
            {
                // Fallback if TempData is not available
                paymentResult = new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Payment completed successfully!",
                    TransactionId = order.TransactionId,
                    TransactionTime = order.OrderDate,
                    Amount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod
                };
            }

            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.Order = order;

            return View("~/Views/user_order_pay/Payment/PaymentSuccess.cshtml", paymentResult);
        }
    }
}