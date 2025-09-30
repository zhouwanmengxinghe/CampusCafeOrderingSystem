using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CampusCafeOrderingSystem.Services.ICreditService _creditService;
        
        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager, CampusCafeOrderingSystem.Services.ICreditService creditService)
        {
            _context = context;
            _userManager = userManager;
            _creditService = creditService;
        }

        // User Dashboard/Index page
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Get user statistics for dashboard
            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == user.Id);
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == user.Id && o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            var feedbackCount = await _context.Feedbacks.CountAsync(f => f.UserId == user.Id);
            
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            var userCredits = await _creditService.GetUserCreditsAsync(user.Id);
            var currentCredits = userCredits?.CurrentCredits ?? 0;

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.FeedbackCount = feedbackCount;
            ViewBag.CurrentCredits = currentCredits;

            return View();
        }
        
        // User Profile page
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            // Get additional user profile data from UserPreference or other sources
            var userPreference = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                FullName = "", // TODO: Add to Identity user or separate table
                StudentId = "", // TODO: Add to Identity user or separate table
                Department = "", // TODO: Add to Identity user or separate table
                Address = userPreference?.DefaultDeliveryAddress ?? string.Empty
            };
            
            return View(model);
        }
        
        [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            // Update basic user info
            user.PhoneNumber = model.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                // Update or create user preference for address
                var userPreference = await _context.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);
                
                if (userPreference == null)
                {
                    userPreference = new UserPreference
                    {
                        UserId = user.Id,
                        DefaultDeliveryAddress = model.Address
                    };
                    _context.UserPreferences.Add(userPreference);
                }
                else
                {
                    userPreference.DefaultDeliveryAddress = model.Address;
                    userPreference.UpdatedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
            }
            
            return RedirectToAction(nameof(Profile));
        }
        

        
        // User Preferences page
        public async Task<IActionResult> Preferences()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
                
            if (preferences == null)
            {
                preferences = new UserPreference
                {
                    UserId = user.Id,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    Theme = "Light",
                    Language = "English",
                    PreferredPaymentMethod = "CampusCard"
                };
                _context.UserPreferences.Add(preferences);
                await _context.SaveChangesAsync();
            }
            
            return View(preferences);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preferences(UserPreference model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (preferences == null)
            {
                model.UserId = user.Id;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                _context.UserPreferences.Add(model);
            }
            else
            {
                preferences.EmailNotifications = model.EmailNotifications;
                preferences.SmsNotifications = model.SmsNotifications;
                preferences.OrderStatusNotifications = model.OrderStatusNotifications;
                preferences.PromotionNotifications = model.PromotionNotifications;
                preferences.DietaryRestrictions = model.DietaryRestrictions;
                preferences.FavoriteCategories = model.FavoriteCategories;
                preferences.AllergyInformation = model.AllergyInformation;
                preferences.Theme = model.Theme;
                preferences.Language = model.Language;
                preferences.PreferredPaymentMethod = model.PreferredPaymentMethod;
                preferences.DefaultDeliveryAddress = model.DefaultDeliveryAddress;
                preferences.DeliveryInstructions = model.DeliveryInstructions;
                preferences.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Preferences updated successfully!";
            return RedirectToAction(nameof(Preferences));
        }
        
        // Redirect to existing Orders page
        public IActionResult Orders()
        {
            return RedirectToAction("MyOrders", "Order");
        }
        
        // User Credits page
        public async Task<IActionResult> Credits()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            // 获取或创建用户积分记录
            var userCredit = await _context.UserCredits
                .FirstOrDefaultAsync(uc => uc.UserId == user.Id);
                
            if (userCredit == null)
            {
                // 创建新的用户积分记录
                userCredit = new UserCredit
                {
                    UserId = user.Id,
                    CurrentCredits = 0,
                    TotalEarned = 0,
                    TotalSpent = 0
                };
                _context.UserCredits.Add(userCredit);
                await _context.SaveChangesAsync();
            }
            
            // 获取积分历史记录
            var creditHistory = await _context.CreditHistories
                .Where(ch => ch.UserId == user.Id)
                .OrderByDescending(ch => ch.CreatedAt)
                .Take(20) // 最近20条记录
                .Select(ch => new CreditTransaction
                {
                    Id = ch.Id,
                    Date = ch.CreatedAt,
                    Description = ch.Description,
                    Amount = (int)ch.Amount,
                    Type = ch.Type,
                    BalanceAfter = (int)ch.BalanceAfter
                })
                .ToListAsync();
            
            var model = new UserCreditsViewModel
            {
                UserId = user.Id,
                CurrentCredits = (int)userCredit.CurrentCredits,
                TotalEarned = (int)userCredit.TotalEarned,
                TotalSpent = (int)userCredit.TotalSpent,
                CreditHistory = creditHistory
            };
            
            return View(model);
        }
        
        // User History page
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Take(50) // Limit to recent 50 orders
                .Select(o => new OrderSummary
                {
                    Id = o.Id,
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    OrderItems = o.OrderItems.Select(oi => oi.MenuItemName).ToList(),
                    PaymentMethod = o.PaymentMethod
                })
                .ToListAsync();
                
            var feedbacks = await _context.Feedbacks
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.CreatedAt)
                .Take(20) // Limit to recent 20 feedbacks
                .Select(f => new FeedbackSummary
                {
                    FeedbackId = f.Id,
                    CreatedAt = f.CreatedAt,
                    Subject = f.Subject,
                    Status = f.Status,
                    Category = f.Category.ToString(),
                    Message = f.Message,
                    Rating = f.Rating,
                    AdminReply = f.AdminResponse ?? string.Empty,
                    RepliedAt = f.ResponseDate
                })
                .ToListAsync();
                
            var model = new UserHistoryViewModel
            {
                UserId = user.Id,
                RecentOrders = orders,
                RecentFeedbacks = feedbacks
            };
            
            return View(model);
        }
    }
    
    // UserProfileViewModel is defined in Models/ViewModels/UserProfileViewModel.cs
    
    // UserCreditsViewModel, CreditTransaction, and UserHistoryViewModel are defined in Models/UserViewModel.cs
}