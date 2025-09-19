using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampusCafeOrderingSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET: /Menu
        public async Task<IActionResult> Index(string category = null)
        {
            var query = _context.MenuItems.Where(m => m.IsAvailable);
            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category == category);
            }
            
            var menuItems = await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
            
            // 如果数据库为空，添加示例数据
            if (!menuItems.Any())
            {
                await SeedSampleMenuItems();
                menuItems = await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
            }
            
            ViewBag.Categories = await _context.MenuItems
                .Where(m => m.IsAvailable)
                .Select(m => m.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
                
            return View("~/Views/user_order_pay/Menu/userIndex.cshtml", menuItems);
        }
        
        // GET: /Menu/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == id && m.IsAvailable);
                
            if (menuItem == null)
            {
                return NotFound();
            }
            
            return View(menuItem);
        }
        
        private async Task SeedSampleMenuItems()
        {
            var sampleItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Latte",
                    Description = "A smooth blend of espresso and steamed milk.",
                    Price = 4.5M,
                    ImageUrl = "/images/latte.jpg",
                    Category = "Coffee",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new MenuItem
                {
                    Name = "Cappuccino",
                    Description = "Espresso with frothy milk.",
                    Price = 4.0M,
                    ImageUrl = "/images/cappuccino.jpg",
                    Category = "Coffee",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new MenuItem
                {
                    Name = "Espresso",
                    Description = "Strong classic espresso shot.",
                    Price = 3.0M,
                    ImageUrl = "/images/espresso.jpg",
                    Category = "Coffee",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new MenuItem
                {
                    Name = "Croissant",
                    Description = "Buttery, flaky pastry perfect with coffee.",
                    Price = 3.5M,
                    ImageUrl = "/images/croissant.jpg",
                    Category = "Pastry",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new MenuItem
                {
                    Name = "Caesar Salad",
                    Description = "Fresh romaine lettuce with caesar dressing.",
                    Price = 8.5M,
                    ImageUrl = "/images/caesar-salad.jpg",
                    Category = "Salad",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
            
            _context.MenuItems.AddRange(sampleItems);
            await _context.SaveChangesAsync();
        }
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetString("Cart");
        
            return View();
        }
    }
}