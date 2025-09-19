using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Services;
using CampusCafeOrderingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
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
        public IActionResult Vendors()
        {
            // TODO: Replace with database query
            var demo = new List<VendorViewModel>
            {
                new VendorViewModel { Id="V001", Name="Cafe Mocha",      Email="mocha@example.com",    Phone="021-000-111", Status="Active",   RegisteredAt=new DateTime(2025,7,15), Address="Campus Center 1F", MenuItems=24, Rating=4.6m },
                new VendorViewModel { Id="V002", Name="Green Tea House", Email="greentea@example.com", Phone="021-000-222", Status="Pending",  RegisteredAt=new DateTime(2025,7,20), Address="Library Annex",   MenuItems=12, Rating=4.2m },
                new VendorViewModel { Id="V003", Name="Fresh Smoothies", Email="smoothie@example.com", Phone="021-000-333", Status="Active",   RegisteredAt=new DateTime(2025,7,25), Address="Gym Lobby",       MenuItems=15, Rating=4.8m },
                new VendorViewModel { Id="V004", Name="Baker’s Choice",  Email="baker@example.com",    Phone="021-000-444", Status="Disabled", RegisteredAt=new DateTime(2025,7,26), Address="Dorm A",          MenuItems=18, Rating=4.1m },
            };

            return View(demo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveVendor(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["VendorToast"] = "⚠️ Invalid vendor id.";
                return RedirectToAction(nameof(Vendors));
            }

            // TODO: Update database status to Active
            TempData["VendorToast"] = $"✅ Vendor {id} approved.";
            return RedirectToAction(nameof(Vendors));
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
        public IActionResult ToggleVendor(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["VendorToast"] = "⚠️ Invalid vendor id.";
                return RedirectToAction(nameof(Vendors));
            }

            // TODO: Read current status and toggle: Active -> Disabled; Disabled/Rejected/Pending -> Active
            TempData["VendorToast"] = $"↔️ Vendor {id} status changed.";
            return RedirectToAction(nameof(Vendors));
        }

        // ========= Order Management =========
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                TempData["Toast"] = "❌ Order not found.";
                return RedirectToAction(nameof(Orders));
            }

            var oldStatus = order.Status;
            order.Status = status;

            // Set estimated completion time when order is confirmed
            if (status == OrderStatus.Confirmed && !order.EstimatedCompletionTime.HasValue)
            {
                order.EstimatedCompletionTime = DateTime.Now.AddMinutes(15); // Default 15 minutes
            }

            // Set completed time when order is completed
            if (status == OrderStatus.Completed && !order.CompletedTime.HasValue)
            {
                order.CompletedTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ Order {order.OrderNumber} status updated from {oldStatus} to {status}.";
            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetEstimatedTime(int orderId, int minutes)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                TempData["Toast"] = "❌ Order not found.";
                return RedirectToAction(nameof(Orders));
            }

            order.EstimatedCompletionTime = DateTime.Now.AddMinutes(minutes);
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ Estimated completion time set to {minutes} minutes for order {order.OrderNumber}.";
            return RedirectToAction(nameof(Orders));
        }

        // ========= Reports =========
        public IActionResult Reports()
        {
            return View();
        }

        // ========= Review Center =========
        [HttpGet]
        public IActionResult ReviewCenter()
        {
            // TODO: Replace with database query
            var demo = new List<ReviewViewModel>
            {
                new ReviewViewModel { Id=101, CustomerName="John Doe",  Comment="The coffee was great, but the delivery was slow.", Rating=4, CreatedAt=new DateTime(2025,8,1), Status="Pending" },
                new ReviewViewModel { Id=102, CustomerName="Jane Smith",Comment="Excellent service and delicious food!",             Rating=5, CreatedAt=new DateTime(2025,8,2), Status="Pending" },
                new ReviewViewModel { Id=103, CustomerName="Mike Johnson", Comment="The food was cold when it arrived.",            Rating=2, CreatedAt=new DateTime(2025,8,3), Status="Pending" },
            };

            return View(demo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModerateReview(int id, string decision)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(decision))
            {
                TempData["Toast"] = "⚠️ Invalid review request.";
                return RedirectToAction(nameof(ReviewCenter));
            }

            decision = decision.Equals("Approve", StringComparison.OrdinalIgnoreCase)
                ? "Approved"
                : decision.Equals("Reject", StringComparison.OrdinalIgnoreCase)
                    ? "Rejected"
                    : "Pending";

            // TODO: Update Review.Status = decision based on id
            TempData["Toast"] = decision == "Approved" ? "✅ Review approved." :
                                decision == "Rejected" ? "❌ Review rejected." :
                                "⚠️ No change.";
            return RedirectToAction(nameof(ReviewCenter));
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
