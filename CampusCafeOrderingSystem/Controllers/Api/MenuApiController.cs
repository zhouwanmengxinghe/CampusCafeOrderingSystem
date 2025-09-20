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
                return ApiResponse<PagedResult<MenuItemResponseDto>>.Error("无法获取商家信息", 401);
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
                    return Unauthorized(new { message = "无法获取商家信息" });
                }

                var item = await _menuService.GetMenuItemByIdAsync(id);
                if (item == null || item.VendorEmail != vendorEmail)
                {
                    return NotFound(new { message = "菜品不存在" });
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取菜品详情失败", error = ex.Message });
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
                    return Unauthorized(new { message = "无法获取商家信息" });
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
                return StatusCode(500, new { message = "创建菜品失败", error = ex.Message });
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
                    return Unauthorized(new { message = "无法获取商家信息" });
                }

                // 先检查菜品是否属于当前商家
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null || existingItem.VendorEmail != vendorEmail)
                {
                    return NotFound(new { message = "菜品不存在" });
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
                    return NotFound(new { message = "菜品不存在" });
                }

                return Ok(updatedItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新菜品失败", error = ex.Message });
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
                    return Unauthorized(new { message = "无法获取商家信息" });
                }

                // 先检查菜品是否属于当前商家
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null || existingItem.VendorEmail != vendorEmail)
                {
                    return NotFound(new { message = "菜品不存在" });
                }

                var success = await _menuService.DeleteMenuItemAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "菜品不存在" });
                }

                return Ok(new { message = "菜品删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除菜品失败", error = ex.Message });
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
                    return Unauthorized(new { message = "无法获取商家信息" });
                }

                // 先检查菜品是否属于当前商家
                var existingItem = await _menuService.GetMenuItemByIdAsync(id);
                if (existingItem == null || existingItem.VendorEmail != vendorEmail)
                {
                    return NotFound(new { message = "菜品不存在" });
                }

                var success = await _menuService.ToggleMenuItemStatusAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "菜品不存在" });
                }

                return Ok(new { message = "菜品状态更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新菜品状态失败", error = ex.Message });
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
                    return Unauthorized(new { message = "无法获取商家信息" });
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
                return StatusCode(500, new { message = "获取分类列表失败", error = ex.Message });
            }
        }
    }

    public class CreateMenuItemRequest
    {
        [Required(ErrorMessage = "菜品名称不能为空")]
        [StringLength(100, ErrorMessage = "菜品名称不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述不能超过500个字符")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "价格不能为空")]
        [Range(0.01, 999.99, ErrorMessage = "价格必须在0.01到999.99之间")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "分类不能为空")]
        [StringLength(50, ErrorMessage = "分类不能超过50个字符")]
        public string Category { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "图片URL不能超过200个字符")]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateMenuItemRequest
    {
        [Required(ErrorMessage = "菜品名称不能为空")]
        [StringLength(100, ErrorMessage = "菜品名称不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述不能超过500个字符")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "价格不能为空")]
        [Range(0.01, 999.99, ErrorMessage = "价格必须在0.01到999.99之间")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "分类不能为空")]
        [StringLength(50, ErrorMessage = "分类不能超过50个字符")]
        public string Category { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "图片URL不能超过200个字符")]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}