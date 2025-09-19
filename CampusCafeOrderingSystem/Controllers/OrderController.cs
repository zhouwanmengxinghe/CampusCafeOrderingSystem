using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace CampusCafeOrderingSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        // My Orders page
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
                
            return View(orders);
        }
        
        // Order Details page
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);
                
            if (order == null)
            {
                return NotFound();
            }
            
            return View(order);
        }
        
        // Track order by order number
        public async Task<IActionResult> Track(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                TempData["Error"] = "Please enter a valid order number.";
                return RedirectToAction("TrackOrder");
            }
            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
                
            if (order == null)
            {
                TempData["Error"] = "Order not found. Please check your order number and try again.";
                return RedirectToAction("TrackOrder");
            }
            
            return View("OrderTracking", order);
        }
        
        // Order tracking page (no login required)
        public IActionResult TrackOrder()
        {
            return View();
        }
        
        // API to get order status (for real-time updates)
        [HttpGet]
        public async Task<IActionResult> GetOrderStatus(string orderNumber)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
                
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }
            
            return Json(new 
            { 
                success = true, 
                status = order.Status.ToString(),
                estimatedTime = order.EstimatedCompletionTime?.ToString("HH:mm"),
                completedTime = order.CompletedTime?.ToString("HH:mm")
            });
        }
    }
}