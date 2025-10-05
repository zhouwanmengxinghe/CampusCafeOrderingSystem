using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusCafeOrderingSystem.Services;
using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Vendor")]
    public class MenuApiController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuApiController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<MenuItemResponseDto>>>> GetMenuItems(
            [FromQuery] MenuItemQueryDto query)
        {
            var vendorEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(vendorEmail))
            {
                return ApiResponse<PagedResult<MenuItemResponseDto>>.Error("Unable to get vendor information", 401);
            }

            IEnumerable<MenuItem> items;

            if (query.IsAvailable == true)
            {
                items = await _menuService.GetAvailableMenuItemsByVendorAsync(vendorEmail);
            }
            else
            {
                items = await _menuService.GetMenuItemsByVendorAsync(vendorEmail);
            }

            if (!string.IsNullOrEmpty(query.Search))
            {
                items = items.Where(i => i.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                                       i.Description.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                                       i.Category.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(query.Category))
            {
                items = items.Where(i => i.Category == query.Category);
            }

            if (query.IsAvailable.HasValue)
            {
                items = items.Where(i => i.IsAvailable == query.IsAvailable.Value);
            }

            var itemList = items.ToList();
            var totalCount = itemList.Count;
            
            // Apply pagination
            var pagedItems = itemList
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(i => new MenuItemResponseDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    Category = i.Category,
                    ImageUrl = i.ImageUrl,
                    IsAvailable = i.IsAvailable,
                    VendorEmail = i.VendorEmail
                })
                .ToList();

            var result = new PagedResult<MenuItemResponseDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                CurrentPage = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            };

            return ApiResponse<PagedResult<MenuItemResponseDto>>.SuccessResult(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItem>> GetMenuItem(int id)
        {
            try
            {
                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { message = "Unable to get vendor information" });
                }

                var item = await _menuService.GetMenuItemByIdAsync(id);
                if (item == null || item.VendorEmail != vendorEmail)
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

        [HttpPost]
        public async Task<ActionResult<MenuItem>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { message = "Unable to get vendor information" });
                }

                var menuItem = new MenuItem
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    Price = request.Price,
                    Category = request.Category,
                    ImageUrl = request.ImageUrl ?? "/images/placeholder.jpg",
                    IsAvailable = request.IsAvailable,
                    VendorEmail = vendorEmail
                };

                var createdItem = await _menuService.CreateMenuItemAsync(menuItem);
                return CreatedAtAction(nameof(GetMenuItem), new { id = createdItem.Id }, createdItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create menu item", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MenuItem>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { message = "Unable to get vendor information" });
                }

                // First check if the menu item belongs to the current vendor
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null || existingItem.VendorEmail != vendorEmail)
                {
                    return NotFound(new { message = "Menu item not found" });
                }

                var menuItem = new MenuItem
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    Price = request.Price,
                    Category = request.Category,
                    ImageUrl = request.ImageUrl ?? "/images/placeholder.jpg",
                    IsAvailable = request.IsAvailable,
                    VendorEmail = vendorEmail
                };

                var updatedItem = await _menuService.UpdateMenuItemAsync(id, menuItem);
                if (updatedItem == null)
                {
                    return NotFound(new { message = "Menu item not found" });
                }

                return Ok(updatedItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update menu item", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { message = "Unable to get vendor information" });
                }

                // First check if the menu item belongs to the current vendor
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null || existingItem.VendorEmail != vendorEmail)
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

        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult> ToggleMenuItemStatus(int id)
        {
            try
            {
                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { message = "Unable to get vendor information" });
                }

                // First check if the menu item belongs to the current vendor
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null || existingItem.VendorEmail != vendorEmail)
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

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { message = "Unable to get vendor information" });
                }

                var items = await _menuService.GetMenuItemsByVendorAsync(vendorEmail);
                var categories = items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Category))
                    .Select(i => i.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get category list", error = ex.Message });
            }
        }
    }

    public class CreateMenuItemRequest
    {
        [Required(ErrorMessage = "Menu item name cannot be empty")]
        [StringLength(100, ErrorMessage = "Menu item name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price cannot be empty")]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between 0.01 and 999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category cannot be empty")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Image URL cannot exceed 200 characters")]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateMenuItemRequest
    {
        [Required(ErrorMessage = "Menu item name cannot be empty")]
        [StringLength(100, ErrorMessage = "Menu item name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price cannot be empty")]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between 0.01 and 999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category cannot be empty")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Image URL cannot exceed 200 characters")]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}