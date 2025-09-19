using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Services
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync();
        Task<IEnumerable<MenuItem>> GetAvailableMenuItemsAsync();
        Task<IEnumerable<MenuItem>> GetMenuItemsByVendorAsync(string vendorEmail);
        Task<IEnumerable<MenuItem>> GetAvailableMenuItemsByVendorAsync(string vendorEmail);
        Task<MenuItem?> GetMenuItemByIdAsync(int id);
        Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem);
        Task<MenuItem?> UpdateMenuItemAsync(int id, MenuItem menuItem);
        Task<bool> DeleteMenuItemAsync(int id);
        Task<IEnumerable<MenuItem>> SearchMenuItemsAsync(string searchTerm);
        Task<IEnumerable<MenuItem>> GetMenuItemsByCategoryAsync(string category);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<bool> ToggleMenuItemStatusAsync(int id);
    }
}