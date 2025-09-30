using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.DTOs
{
    /// <summary>
    /// Create menu item request DTO
    /// </summary>
    public class CreateMenuItemRequestDto
    {
        [Required(ErrorMessage = "Menu item name is required")]
        [StringLength(100, ErrorMessage = "Menu item name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Menu item description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid image URL format")]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Range(0, int.MaxValue, ErrorMessage = "Preparation time cannot be negative")]
        public int? PreparationTime { get; set; }

        [StringLength(500, ErrorMessage = "Allergen information cannot exceed 500 characters")]
        public string? Allergens { get; set; }

        [StringLength(200, ErrorMessage = "Nutritional information cannot exceed 200 characters")]
        public string? NutritionalInfo { get; set; }
    }

    /// <summary>
    /// Update menu item request DTO
    /// </summary>
    public class UpdateMenuItemRequestDto
    {
        [StringLength(100, ErrorMessage = "Menu item name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Menu item description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal? Price { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string? Category { get; set; }

        [Url(ErrorMessage = "Invalid image URL format")]
        public string? ImageUrl { get; set; }

        public bool? IsAvailable { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Preparation time cannot be negative")]
        public int? PreparationTime { get; set; }

        [StringLength(500, ErrorMessage = "Allergen information cannot exceed 500 characters")]
        public string? Allergens { get; set; }

        [StringLength(200, ErrorMessage = "Nutritional information cannot exceed 200 characters")]
        public string? NutritionalInfo { get; set; }
    }

    /// <summary>
    /// 菜品响应DTO
    /// </summary>
    public class MenuItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public string? Allergens { get; set; }
        public string? NutritionalInfo { get; set; }
        public string VendorEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// Menu query parameters DTO
    /// </summary>
    public class MenuQueryParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;

        [StringLength(50, ErrorMessage = "Category filter cannot exceed 50 characters")]
        public string? Category { get; set; }

        public bool? IsAvailable { get; set; }

        [StringLength(100, ErrorMessage = "Search keyword cannot exceed 100 characters")]
        public string? Search { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Minimum price must be between 0.01 and 9999.99")]
        public decimal? MinPrice { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Maximum price must be between 0.01 and 9999.99")]
        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; } = "name"; // name, price, rating, created

        public string? SortOrder { get; set; } = "asc"; // asc, desc
    }

    /// <summary>
    /// Menu category response DTO
    /// </summary>
    public class MenuCategoryResponse
    {
        public string Name { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public int AvailableItemCount { get; set; }
    }

    /// <summary>
    /// Batch update availability request DTO
    /// </summary>
    public class BatchUpdateAvailabilityRequest
    {
        [Required(ErrorMessage = "Menu item ID list is required")]
        [MinLength(1, ErrorMessage = "At least one menu item ID is required")]
        public List<int> MenuItemIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Availability status is required")]
        public bool IsAvailable { get; set; }
    }

    /// <summary>
    /// Menu item query DTO
    /// </summary>
    public class MenuItemQueryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;

        [StringLength(50, ErrorMessage = "Category filter cannot exceed 50 characters")]
        public string? Category { get; set; }

        public bool? IsAvailable { get; set; }

        [StringLength(100, ErrorMessage = "Search keyword cannot exceed 100 characters")]
        public string? Search { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Minimum price must be between 0.01 and 9999.99")]
        public decimal? MinPrice { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Maximum price must be between 0.01 and 9999.99")]
        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; } = "name"; // name, price, rating, created

        public string? SortOrder { get; set; } = "asc"; // asc, desc
    }

    /// <summary>
    /// Menu item response DTO
    /// </summary>
    public class MenuItemResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public string? Allergens { get; set; }
        public string? NutritionalInfo { get; set; }
        public string VendorEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}