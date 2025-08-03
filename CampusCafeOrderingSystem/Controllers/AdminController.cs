using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Models.Entities; // 引用实体类命名空间
using CampusCafeOrderingSystem.Models; // 如果 Order 保留在 Models 根目录


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

        // 1. 管理员控制台首页
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // 2. 用户管理列表
        public IActionResult ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // 3. 分配用户角色
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        // 4. 查看用户详情
        public async Task<IActionResult> ManageUserDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // 5. 显示所有供应商（审核用）
        public async Task<IActionResult> ManageVendors()
        {
            var users = _userManager.Users.ToList();
            var vendors = new List<IdentityUser>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Vendor"))
                {
                    vendors.Add(user);
                }
            }

            return View(vendors);
        }

        // 6. 查看供应商详情
        public async Task<IActionResult> ManageVendorDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // 7. 供应商资质审核（占位）
        public IActionResult VendorQualification(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            // TODO: 实现 VendorDocuments 表及其数据加载
            ViewBag.VendorId = id;
            return View();
        }

        // 8. 查看所有订单
        public IActionResult ManageOrders()
        {
            var orders = _context.Orders.ToList();
            return View(orders);
        }

        // 9. 修改订单状态
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = newStatus;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(ManageOrders));
        }

        // 10. 系统统计报表页面
        public IActionResult ManageReports()
        {
            var totalOrders = _context.Orders.Count();
            var completedOrders = _context.Orders.Count(o => o.Status == "已完成");
            var pendingOrders = _context.Orders.Count(o => o.Status == "待处理");

            ViewBag.TotalOrders = totalOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.PendingOrders = pendingOrders;

            return View();
        }

        // 11. 团体订餐审批页面（占位）
        public IActionResult GroupOrderApprovals()
        {
            // TODO: 实现 GroupOrders 表及审批流程
            return View();
        }

        // 12. 团体订餐详情（占位）
        public IActionResult GroupOrderDetails(int id)
        {
            // TODO: 加载指定 id 的 GroupOrder 数据
            return View();
        }

        // 13. 系统设置页面（GET）
        public IActionResult Settings()
        {
            var settings = _context.SystemSettingsEntity.FirstOrDefault();
            if (settings == null)
            {
                settings = new SystemSettingsEntity
                {
                    BusinessHours = "周一至周五 8:00-18:00",
                    EnableVendorRegistration = true,
                    MaxGroupOrderSize = 100
                };
                _context.SystemSettingsEntity.Add(settings);
                _context.SaveChanges();
            }

            var vm = new SystemSettingsViewModel
            {
                BusinessHours = settings.BusinessHours,
                EnableVendorRegistration = settings.EnableVendorRegistration,
                MaxGroupOrderSize = settings.MaxGroupOrderSize
            };

            return View(vm);
        }

        // 14. 系统设置页面（POST）
        [HttpPost]
        public IActionResult SaveSettings(SystemSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Settings", model);

            var settings = _context.SystemSettingsEntity.FirstOrDefault();
            if (settings == null)
            {
                settings = new SystemSettingsEntity();
                _context.SystemSettingsEntity.Add(settings);
            }

            settings.BusinessHours = model.BusinessHours;
            settings.EnableVendorRegistration = model.EnableVendorRegistration;
            settings.MaxGroupOrderSize = model.MaxGroupOrderSize;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "系统设置已成功保存";
            return RedirectToAction(nameof(Settings));
        }
    }
}
