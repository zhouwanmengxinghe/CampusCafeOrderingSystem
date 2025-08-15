using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
            // TODO: 替换为数据库查询
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

            // TODO: 更新数据库状态为 Active
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

            // TODO: 更新数据库状态为 Rejected
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

            // TODO: 读取当前状态并切换：Active -> Disabled；Disabled/Rejected/Pending -> Active
            TempData["VendorToast"] = $"↔️ Vendor {id} status changed.";
            return RedirectToAction(nameof(Vendors));
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
            // TODO: 替换为数据库查询
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

            // TODO: 根据 id 更新 Review.Status = decision
            TempData["Toast"] = decision == "Approved" ? "✅ Review approved." :
                                decision == "Rejected" ? "❌ Review rejected." :
                                "⚠️ No change.";
            return RedirectToAction(nameof(ReviewCenter));
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
