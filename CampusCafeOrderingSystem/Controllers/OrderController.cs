using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CafeApp.Models.user_order_pay;
using Microsoft.AspNetCore.Authorization;

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
        
        // 我的订单页面
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
        
        // 订单详情页面
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
        
        // 根据订单号跟踪订单
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
        
        // 订单跟踪页面（不需要登录）
        public IActionResult TrackOrder()
        {
            return View();
        }
        
        // 获取订单状态的API（用于实时更新）
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