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
            
            // If database is empty, add sample data
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
                new MenuItem { Name = "Espresso", Description = "A strong, concentrated coffee brewed under high pressure, forming the base for many espresso-based drinks.", Price = 3.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Espresso%20coffee.jpg?width=1200", Category = "Coffee", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Cappuccino", Description = "A 1:1:1 ratio of espresso, steamed milk, and frothy milk foam, creating a smooth and creamy texture.", Price = 4.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Classic%20Cappuccino.jpg?width=1200", Category = "Coffee", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Latte", Description = "Espresso with a larger amount of steamed milk, often topped with latte art for a creamy and mild taste.", Price = 4.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Latte%20art.jpg?width=1200", Category = "Coffee", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Cold Brew", Description = "Coffee brewed with cold water for an extended period, resulting in a smooth, less acidic flavor.", Price = 4.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Cold%20Brew%20Coffee.jpg?width=1200", Category = "Coffee", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Mocha", Description = "A combination of espresso, steamed milk, and chocolate syrup, delivering a sweet and rich flavor.", Price = 4.8M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Mocha%20coffee.jpg?width=1200", Category = "Coffee", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Macchiato", Description = "An espresso with just a touch of milk foam on top, offering a layered and bold flavor.", Price = 3.8M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Caff%C3%A8%20macchiato.jpg?width=1200", Category = "Coffee", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Dessert
                new MenuItem { Name = "NY Cheesecake", Description = "A rich and dense baked cheesecake with a smooth, creamy texture.", Price = 6.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/My%20first%20cheesecake.jpg?width=1200", Category = "Dessert", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Tiramisu", Description = "A classic Italian dessert made with layers of coffee-soaked ladyfingers and mascarpone cheese.", Price = 6.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Tiramisu%20dessert.jpg?width=1200", Category = "Dessert", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Brownie", Description = "A chocolatey, dense dessert with a fudgy texture, often paired with ice cream.", Price = 4.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Brownie%20dessert%20(43110731564).jpg?width=1200", Category = "Dessert", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Macaron", Description = "A delicate French pastry made with almond meringue, filled with buttercream or ganache.", Price = 3.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Macaron%201.jpg?width=1200", Category = "Dessert", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Apple Pie", Description = "A traditional American dessert with a flaky pie crust and spiced apple filling.", Price = 5.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/American%20apple%20pie.jpg?width=1200", Category = "Dessert", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Panna Cotta", Description = "A creamy, Italian dessert made from sweetened cream, often served with berries or fruit sauce.", Price = 5.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Panna%20Cotta.JPG?width=1200", Category = "Dessert", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Snack
                new MenuItem { Name = "Club Sandwich", Description = "A classic triple-layer sandwich with bacon, chicken, lettuce, and tomato, often served with fries.", Price = 8.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Club-sandwich.jpg?width=1200", Category = "Snack", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Bagel Sandwich", Description = "A sandwich made with a bagel, filled with bacon, eggs, and cheese, offering a hearty breakfast.", Price = 6.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/BEC%20Sandwich%20on%20Everything%20Bagel.jpg?width=1200", Category = "Snack", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Quiche Lorraine", Description = "A savory pie filled with eggs, cream, cheese, and bacon — a classic French dish.", Price = 7.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Quiche%20lorraine%2001.JPG?width=1200", Category = "Snack", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Pretzel", Description = "A twisted dough snack, often sprinkled with salt, perfect for pairing with coffee.", Price = 3.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Pretzel.jpg?width=1200", Category = "Snack", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Scone", Description = "A British baked good, typically served with clotted cream and jam, often enjoyed with tea.", Price = 3.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Scone%20varieties.jpg?width=1200", Category = "Snack", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Yogurt Parfait", Description = "A layered snack of yogurt, granola, and fresh fruit, offering a healthy and refreshing option.", Price = 5.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/-2020-01-13%20Fruit%20and%20Yogurt%20Breakfast%20Parfait%2C%20Trimingham.JPG?width=1200", Category = "Snack", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Tea
                new MenuItem { Name = "Earl Grey", Description = "A black tea flavored with bergamot orange, offering a fragrant, citrusy taste.", Price = 3.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Cup%20of%20Earl%20Gray.jpg?width=1200", Category = "Tea", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Sencha", Description = "A Japanese green tea, known for its grassy, fresh flavor, often enjoyed after meals.", Price = 3.2M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Sencha%20tea.jpg?width=1200", Category = "Tea", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Oolong", Description = "A partially fermented tea, blending floral and roasted notes for a complex flavor profile.", Price = 3.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Oolong%20Tea.jpg?width=1200", Category = "Tea", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Matcha Latte", Description = "A combination of matcha green tea and steamed milk, often served with vibrant green color and artful presentation.", Price = 4.5M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Matcha%20Latte.jpg?width=1200", Category = "Tea", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Masala Chai", Description = "A spiced tea made with black tea, milk, and aromatic spices, providing warmth and comfort.", Price = 4.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Masala%20Tea%201.jpg?width=1200", Category = "Tea", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new MenuItem { Name = "Chamomile", Description = "A caffeine-free herbal tea, known for its calming properties and light floral flavor.", Price = 3.0M, ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Glass%20of%20chamomile%20tea.jpg?width=1200", Category = "Tea", IsAvailable = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
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