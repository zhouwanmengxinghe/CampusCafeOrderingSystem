using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Services;
using CampusCafeOrderingSystem.Hubs;
using System.Security.Claims;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class MerchantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOrderService _orderService;
        private readonly IMenuService _menuService;
        private readonly IHubContext<OrderHub> _hubContext;

        public MerchantController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IOrderService orderService,
            IMenuService menuService,
            IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _orderService = orderService;
            _menuService = menuService;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Debug()
        {
            var debugInfo = new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserName = User.Identity?.Name,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            // Check menu data in database
            var allMenuItems = await _context.MenuItems.ToListAsync();
            var vendorMenuItems = await _context.MenuItems
                .Where(m => m.VendorEmail == debugInfo.Email)
                .ToListAsync();

            ViewBag.DebugInfo = debugInfo;
            ViewBag.AllMenuItems = allMenuItems;
            ViewBag.VendorMenuItems = vendorMenuItems;
            ViewBag.TotalMenuItems = allMenuItems.Count;
            ViewBag.VendorMenuItemsCount = vendorMenuItems.Count;

            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var merchantEmail = user?.Email;

            if (string.IsNullOrEmpty(merchantEmail))
            {
                TempData["ErrorMessage"] = "Unable to get merchant information";
                return View();
            }

            // Get today's order statistics
            var today = DateTime.Today;
            var todayOrders = await _context.Orders
                .Where(o => o.VendorEmail == merchantEmail && o.OrderDate >= today)
                .ToListAsync();

            // Get pending orders
            var pendingOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.VendorEmail == merchantEmail && 
                           (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed))
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            // Get recently completed orders
            var recentCompletedOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.VendorEmail == merchantEmail && o.Status == OrderStatus.Completed)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TodayOrderCount = todayOrders.Count;
            ViewBag.TodayRevenue = todayOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);
            ViewBag.PendingOrderCount = pendingOrders.Count;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.RecentCompletedOrders = recentCompletedOrders;

            return View();
        }

        public async Task<IActionResult> MenuManagement()
        {
            var user = await _userManager.GetUserAsync(User);
            var merchantEmail = user?.Email;

            if (string.IsNullOrEmpty(merchantEmail))
            {
                TempData["ErrorMessage"] = "Unable to get merchant information";
                return View(new List<MenuItem>());
            }

            var menuItems = await _context.MenuItems
                .Where(m => m.VendorEmail == merchantEmail)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return View(menuItems);
        }

        public async Task<IActionResult> OrderManagement()
        {
            var user = await _userManager.GetUserAsync(User);
            var merchantEmail = user?.Email;

            if (string.IsNullOrEmpty(merchantEmail))
            {
                TempData["ErrorMessage"] = "Unable to get merchant information";
                return View(new List<Order>());
            }

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.VendorEmail == merchantEmail)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            var merchantEmail = user?.Email;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.VendorEmail == merchantEmail);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order does not exist or no permission to access";
                return RedirectToAction("OrderManagement");
            }

            var success = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (success)
            {
                TempData["SuccessMessage"] = $"Order {order.OrderNumber} status has been updated to {status}";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update order status";
            }

            return RedirectToAction("OrderManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetEstimatedTime(int orderId, int minutes)
        {
            var user = await _userManager.GetUserAsync(User);
            var merchantEmail = user?.Email;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.VendorEmail == merchantEmail);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order does not exist or no permission to access";
                return RedirectToAction("OrderManagement");
            }

            order.EstimatedCompletionTime = DateTime.Now.AddMinutes(minutes);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order {order.OrderNumber} estimated completion time has been set to {minutes} minutes later";
            return RedirectToAction("OrderManagement");
        }

        public async Task<IActionResult> Reports()
        {
            var user = await _userManager.GetUserAsync(User);
            var merchantEmail = user?.Email;

            if (string.IsNullOrEmpty(merchantEmail))
            {
                TempData["ErrorMessage"] = "Unable to get merchant information";
                return View();
            }

            // Get monthly order statistics
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var nextMonth = currentMonth.AddMonths(1);

            var monthlyOrders = await _context.Orders
                .Where(o => o.VendorEmail == merchantEmail && 
                           o.OrderDate >= currentMonth && 
                           o.OrderDate < nextMonth)
                .ToListAsync();

            ViewBag.MonthlyOrderCount = monthlyOrders.Count;
            ViewBag.MonthlyRevenue = monthlyOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);
            ViewBag.MonthlyOrders = monthlyOrders;

            return View();
        }

        public IActionResult BusinessReports()
        {
            return View();
        }

        public async Task<IActionResult> ReviewManagement()
        {
            var user = await _userManager.GetUserAsync(User);
            var merchantEmail = user?.Email;

            if (string.IsNullOrEmpty(merchantEmail))
            {
                TempData["ErrorMessage"] = "Unable to get merchant information";
                return View(new List<Review>());
            }

            // Get all reviews for this merchant
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.MenuItem)
                .Where(r => r.MenuItem.VendorEmail == merchantEmail)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
                    
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                var oldStatus = order.Status;
                
                // Convert string to OrderStatus enum
                if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    order.Status = orderStatus;
                }
                else
                {
                    return Json(new { success = false, message = "Invalid order status" });
                }
                
                order.UpdatedAt = DateTime.Now;

                // Set estimated completion time based on status
                if (order.Status == OrderStatus.Preparing && !order.EstimatedCompletionTime.HasValue)
                {
                    order.EstimatedCompletionTime = DateTime.Now.AddMinutes(15);
                }
                else if (order.Status == OrderStatus.Completed)
                {
                    order.CompletedTime = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Send SignalR notification to user
                await _hubContext.Clients.User(order.UserId).SendAsync("OrderStatusUpdated", new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    status = order.Status,
                    estimatedTime = order.EstimatedCompletionTime,
                    completedTime = order.CompletedTime
                });

                // Send SignalR notification to administrators
                await _hubContext.Clients.Group("Admins").SendAsync("OrderStatusUpdated", new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    customerName = order.User?.UserName ?? "Unknown",
                    vendorEmail = order.VendorEmail,
                    status = order.Status,
                    oldStatus = oldStatus,
                    updatedAt = order.UpdatedAt
                });

                return Json(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Update failed: " + ex.Message });
            }
        }


    }
}