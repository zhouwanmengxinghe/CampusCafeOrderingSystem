using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusCafeOrderingSystem.Services
{
    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
        {
            return await _context.MenuItems
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetAvailableMenuItemsAsync()
        {
            return await _context.MenuItems
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetMenuItemsByVendorAsync(string vendorEmail)
        {
            return await _context.MenuItems
                .Where(m => m.VendorEmail == vendorEmail)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetAvailableMenuItemsByVendorAsync(string vendorEmail)
        {
            return await _context.MenuItems
                .Where(m => m.VendorEmail == vendorEmail && m.IsAvailable)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<MenuItem?> GetMenuItemByIdAsync(int id)
        {
            return await _context.MenuItems.FindAsync(id);
        }

        public async Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem)
        {
            menuItem.CreatedAt = DateTime.UtcNow;
            menuItem.UpdatedAt = DateTime.UtcNow;
            
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
            
            return menuItem;
        }

        public async Task<MenuItem?> UpdateMenuItemAsync(int id, MenuItem menuItem)
        {
            var existingItem = await _context.MenuItems.FindAsync(id);
            if (existingItem == null)
            {
                return null;
            }

            existingItem.Name = menuItem.Name;
            existingItem.Description = menuItem.Description;
            existingItem.Price = menuItem.Price;
            existingItem.Category = menuItem.Category;
            existingItem.ImageUrl = menuItem.ImageUrl;
            existingItem.IsAvailable = menuItem.IsAvailable;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingItem;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return false;
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MenuItem>> SearchMenuItemsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllMenuItemsAsync();
            }

            return await _context.MenuItems
                .Where(m => m.Name.Contains(searchTerm) || 
                           m.Description.Contains(searchTerm) ||
                           m.Category.Contains(searchTerm))
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> GetMenuItemsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return await GetAllMenuItemsAsync();
            }

            return await _context.MenuItems
                .Where(m => m.Category == category)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.MenuItems
                .Where(m => !string.IsNullOrEmpty(m.Category))
                .Select(m => m.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<bool> ToggleMenuItemStatusAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return false;
            }

            menuItem.IsAvailable = !menuItem.IsAvailable;
            menuItem.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}