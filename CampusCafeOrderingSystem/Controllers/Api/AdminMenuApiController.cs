using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusCafeOrderingSystem.Services;
using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/admin/menu")]
    [Authorize(Roles = "Admin")]
    public class AdminMenuApiController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public AdminMenuApiController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItem>> GetMenuItem(int id)
        {
            try
            {
                var item = await _menuService.GetMenuItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = "Menu item not found" });
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get menu item details", error = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult> ToggleMenuItemStatus(int id)
        {
            try
            {
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null)
                {
                    return NotFound(new { message = "Menu item not found" });
                }

                var success = await _menuService.ToggleMenuItemStatusAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Menu item not found" });
                }

                return Ok(new { message = "Menu item status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update menu item status", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null)
                {
                    return NotFound(new { message = "Menu item not found" });
                }

                var success = await _menuService.DeleteMenuItemAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Menu item not found" });
                }

                return Ok(new { message = "Menu item deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete menu item", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetAllMenuItems()
        {
            try
            {
                var items = await _menuService.GetAllMenuItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get menu items", error = ex.Message });
            }
        }
    }
}