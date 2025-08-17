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
        
        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        // User Profile page
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
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
            
            user.PhoneNumber = model.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update profile.";
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
                
            if (preferences != null)
            {
                preferences.EmailNotifications = model.EmailNotifications;
                preferences.SmsNotifications = model.SmsNotifications;
                preferences.Theme = model.Theme;
                preferences.Language = model.Language;
                preferences.PreferredPaymentMethod = model.PreferredPaymentMethod;
                preferences.DietaryRestrictions = model.DietaryRestrictions;
                preferences.DeliveryInstructions = model.DeliveryInstructions;
                preferences.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Preferences updated successfully!";
            }
            
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
            
            // TODO: Implement actual credits system
            var model = new UserCreditsViewModel
            {
                UserId = user.Id,
                CurrentCredits = 0, // Placeholder
                TotalEarned = 0,    // Placeholder
                TotalSpent = 0,     // Placeholder
                CreditHistory = new List<CreditTransaction>() // Placeholder
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
                .ToListAsync();
                
            var feedbacks = await _context.Feedbacks
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.CreatedAt)
                .Take(20) // Limit to recent 20 feedbacks
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
    
    // View Models
    public class UserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
    
    public class UserCreditsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public decimal CurrentCredits { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalSpent { get; set; }
        public List<CreditTransaction> CreditHistory { get; set; } = new();
    }
    
    public class CreditTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "Earned" or "Spent"
    }
    
    public class UserHistoryViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public List<CafeApp.Models.user_order_pay.Order> RecentOrders { get; set; } = new();
        public List<Feedback> RecentFeedbacks { get; set; } = new();
    }
}