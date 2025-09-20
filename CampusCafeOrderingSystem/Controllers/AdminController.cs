using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Services;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IHubContext<OrderHub> hubContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _hubContext = hubContext;
        }

        // ========= User Management =========
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var vm = new List<UserViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                vm.Add(new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
            {
                TempData["Toast"] = "⚠️ Invalid request.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Toast"] = "❌ User not found.";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                TempData["Toast"] = $"ℹ️ User is not in role '{role}'.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            TempData["Toast"] = result.Succeeded
                ? $"🗑️ Removed role '{role}' from {user.UserName}."
                : $"❌ Failed to remove role. {string.Join("; ", result.Errors.Select(e => e.Description))}";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
            {
                TempData["Toast"] = "⚠️ Invalid request.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Toast"] = "❌ User not found.";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                TempData["Toast"] = $"❌ Role '{role}' does not exist.";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (await _userManager.IsInRoleAsync(user, role))
            {
                TempData["Toast"] = $"ℹ️ User already in role '{role}'.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            TempData["Toast"] = result.Succeeded
                ? $"✅ Assigned role '{role}' to {user.UserName}."
                : $"❌ Failed to assign role. {string.Join("; ", result.Errors.Select(e => e.Description))}";

            return RedirectToAction(nameof(ManageUsers));
        }

        // ========= Vendor Management =========
        public async Task<IActionResult> Vendors()
        {
            // 获取所有具有Vendor角色的用户
            var vendorRole = await _roleManager.FindByNameAsync("Vendor");
            if (vendorRole == null)
            {
                return View(new List<VendorViewModel>());
            }

            var vendorUsers = await _userManager.GetUsersInRoleAsync("Vendor");
            var vendors = new List<VendorViewModel>();

            foreach (var vendor in vendorUsers)
            {
                // 获取该商家的菜单项数量
                var menuItemCount = await _context.MenuItems
                    .CountAsync(m => m.VendorEmail == vendor.Email);

                // 获取该商家的平均评分
                var averageRating = await _context.Reviews
                    .Include(r => r.MenuItem)
                    .Where(r => r.MenuItem.VendorEmail == vendor.Email)
                    .AverageAsync(r => (decimal?)r.Rating) ?? 0;

                // 获取该商家的订单数量
                var orderCount = await _context.Orders
                    .CountAsync(o => o.VendorEmail == vendor.Email);

                // 确定商家状态
                string status;
                if (vendor.LockoutEnd.HasValue && vendor.LockoutEnd > DateTimeOffset.Now)
                {
                    status = "Disabled";
                }
                else if (vendor.EmailConfirmed)
                {
                    status = "Active";
                }
                else
                {
                    status = "Pending";
                }

                vendors.Add(new VendorViewModel
                {
                    Id = vendor.Id,
                    Name = vendor.UserName ?? "Unknown",
                    Email = vendor.Email ?? "",
                    Phone = vendor.PhoneNumber ?? "N/A",
                    Status = status,
                    RegisteredAt = vendor.LockoutEnd?.DateTime ?? DateTime.Now,
                    Address = "Campus Location", // 可以从用户配置文件获取
                    MenuItems = menuItemCount,
                    Rating = averageRating,
                    OrderCount = orderCount
                });
            }

            return View(vendors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVendor(string id)
        {
            try
            {
                // Find user
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Confirm email (indicates approval)
                user.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    // Ensure user has Vendor role
                    if (!await _userManager.IsInRoleAsync(user, "Vendor"))
                    {
                        await _userManager.AddToRoleAsync(user, "Vendor");
                    }

                    return Json(new { success = true, message = "Vendor approved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Update failed: " + string.Join(", ", updateResult.Errors.Select(e => e.Description)) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Operation failed: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectVendor(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["VendorToast"] = "⚠️ Invalid vendor id.";
                return RedirectToAction(nameof(Vendors));
            }

            // TODO: Update database status to Rejected
            TempData["VendorToast"] = $"❌ Vendor {id} rejected.";
            return RedirectToAction(nameof(Vendors));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVendor(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Invalid vendor ID" });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Vendor not found" });
                }

                // Toggle status: Active -> Disabled; Disabled/Pending -> Active
                bool isCurrentlyActive = user.EmailConfirmed && user.LockoutEnd == null;
                
                if (isCurrentlyActive)
                {
                    // Disable vendor: set lockout time to permanent
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    user.LockoutEnabled = true;
                }
                else
                {
                    // Activate vendor: remove lockout and confirm email
                    user.LockoutEnd = null;
                    user.LockoutEnabled = false;
                    user.EmailConfirmed = true;
                    
                    // Ensure user has Vendor role
                    if (!await _userManager.IsInRoleAsync(user, "Vendor"))
                    {
                        await _userManager.AddToRoleAsync(user, "Vendor");
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    string newStatus = isCurrentlyActive ? "disabled" : "activated";
                    return Json(new { success = true, message = $"Vendor status has been {newStatus}" });
                }
                else
                {
                    return Json(new { success = false, message = "Update failed: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Operation failed: " + ex.Message });
            }
        }

        // ========= Order Management =========
        public async Task<IActionResult> Orders()
        {
            // 获取所有订单，包含相关数据
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "订单未找到" });
                }

                // 将字符串转换为OrderStatus枚举
                if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    order.Status = orderStatus;
                }
                else
                {
                    return Json(new { success = false, message = "无效的订单状态" });
                }
                
                order.UpdatedAt = DateTime.Now;

                // 如果状态是"准备中"，设置预计完成时间
                if (order.Status == OrderStatus.Preparing)
                {
                    order.EstimatedCompletionTime = DateTime.Now.AddMinutes(15); // 默认15分钟
                }

                await _context.SaveChangesAsync();

                // 发送SignalR通知
                await _hubContext.Clients.User(order.UserId).SendAsync("OrderStatusUpdated", new
                {
                    orderId = order.Id,
                    status = order.Status,
                    estimatedTime = order.EstimatedCompletionTime
                });

                return Json(new { success = true, message = "订单状态更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "更新失败: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetEstimatedTime(int orderId, int minutes)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "订单未找到" });
                }

                order.EstimatedCompletionTime = DateTime.Now.AddMinutes(minutes);
                order.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // 发送SignalR通知
                await _hubContext.Clients.User(order.UserId).SendAsync("EstimatedTimeUpdated", new
                {
                    orderId = order.Id,
                    estimatedTime = order.EstimatedCompletionTime
                });

                return Json(new { success = true, message = "预计完成时间设置成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "设置失败: " + ex.Message });
            }
        }

        // ========= Reports =========
        public async Task<IActionResult> Reports()
        {
            // 计算总体统计数据
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            var totalOrders = await _context.Orders.CountAsync();
            var totalCustomers = await _userManager.GetUsersInRoleAsync("Customer");
            var totalVendors = await _userManager.GetUsersInRoleAsync("Vendor");

            // 获取过去30天的每日收入数据
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var dailyRevenue = await _context.Orders
                .Where(o => o.CreatedAt >= thirtyDaysAgo && o.Status == OrderStatus.Completed)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailyRevenueData
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // 获取热门商品数据
            var popularItems = await _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.MenuItem.Name)
                .Select(g => new PopularItemData
                {
                    ItemName = g.Key,
                    OrderCount = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(p => p.OrderCount)
                .Take(10)
                .ToListAsync();

            // 获取商家表现数据
            var vendorPerformance = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .GroupBy(o => o.VendorEmail)
                .Select(g => new VendorPerformanceData
                {
                    VendorName = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageRating = _context.Reviews
                        .Where(r => r.MenuItem.VendorEmail == g.Key)
                        .Average(r => (decimal?)r.Rating) ?? 0
                })
                .OrderByDescending(v => v.Revenue)
                .ToListAsync();

            var reportModel = new ReportViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers.Count,
                TotalVendors = totalVendors.Count,
                DailyRevenue = dailyRevenue,
                PopularItems = popularItems,
                VendorPerformance = vendorPerformance
            };

            return View(reportModel);
        }

        // ========= Review Center =========
        public async Task<IActionResult> ReviewCenter()
        {
            // 获取所有评论，包含相关数据
            var reviews = await _context.Reviews
                .Include(r => r.MenuItem)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    MenuItemName = r.MenuItem.Name,
                    CustomerName = r.User.UserName ?? "Unknown",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsApproved = r.Status == ReviewStatus.Replied,
                    VendorEmail = r.MenuItem.VendorEmail
                })
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReview(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return Json(new { success = false, message = "评论未找到" });
                }

                review.Status = ReviewStatus.Replied;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "评论审核通过" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "操作失败: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectReview(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return Json(new { success = false, message = "评论未找到" });
                }

                review.Status = ReviewStatus.Hidden;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "评论已拒绝" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "操作失败: " + ex.Message });
            }
        }

        // ========= Menu Management =========
        public async Task<IActionResult> MenuManagement()
        {
            var menuItems = await _context.MenuItems
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return View(menuItems);
        }

        // ========= Catering Management =========
        public async Task<IActionResult> CateringApplications()
        {
            var applications = await _context.CateringApplications
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCateringApplication(int id)
        {
            var application = await _context.CateringApplications.FindAsync(id);
            if (application == null)
            {
                TempData["Toast"] = "❌ Application not found.";
                return RedirectToAction(nameof(CateringApplications));
            }

            application.Status = ApplicationStatus.Approved;
            application.ReviewedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ Catering application {application.Id} approved.";
            return RedirectToAction(nameof(CateringApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCateringApplication(int id, string reason = null)
        {
            var application = await _context.CateringApplications.FindAsync(id);
            if (application == null)
            {
                TempData["Toast"] = "❌ Application not found.";
                return RedirectToAction(nameof(CateringApplications));
            }

            application.Status = ApplicationStatus.Rejected;
            application.ReviewedAt = DateTime.Now;
            if (!string.IsNullOrEmpty(reason))
            {
                application.AdminNotes = reason;
            }
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"❌ Catering application {application.Id} rejected.";
            return RedirectToAction(nameof(CateringApplications));
        }

        // ========= Settings =========
        [HttpGet]
        public IActionResult Settings()
        {
            var model = AppSettingsStore.Load();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AppSettingsStore.Save(model);
            TempData["SettingsSaved"] = "✅ Settings have been saved successfully.";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetSettings()
        {
            AppSettingsStore.Reset();
            TempData["SettingsSaved"] = "↩️ Settings have been reset to defaults.";
            return RedirectToAction(nameof(Settings));
        }
    }
}
