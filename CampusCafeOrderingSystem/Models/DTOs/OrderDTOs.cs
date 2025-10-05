using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.DTOs
{
    /// <summary>
    /// Create order request DTO
    /// </summary>
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Order items cannot be empty")]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();

        [Required(ErrorMessage = "Delivery type cannot be empty")]
        public string DeliveryType { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Delivery address cannot exceed 500 characters")]
        public string? DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Payment method cannot be empty")]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Vendor email cannot be empty")]
        [EmailAddress(ErrorMessage = "Vendor email format is incorrect")]
        public string VendorEmail { get; set; } = string.Empty;
    }

    /// <summary>
    /// Create order item request DTO
    /// </summary>
    public class CreateOrderItemRequest
    {
        [Required(ErrorMessage = "Menu item ID cannot be empty")]
        [Range(1, int.MaxValue, ErrorMessage = "Menu item ID must be greater than 0")]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "Quantity cannot be empty")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1-100")]
        public int Quantity { get; set; }

        [MaxLength(500, ErrorMessage = "Special instructions cannot exceed 500 characters")]
        public string? SpecialInstructions { get; set; }
    }

    /// <summary>
    /// Update order status request DTO
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "Order status cannot be empty")]
        public string Status { get; set; } = string.Empty;

        public DateTime? EstimatedCompletionTime { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Order response DTO
    /// </summary>
    public class OrderResponse
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
        public string? DeliveryAddress { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public string? Notes { get; set; }
        public string VendorEmail { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
    }

    /// <summary>
    /// Order item response DTO
    /// </summary>
    public class OrderItemResponse
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    /// <summary>
    /// Order statistics response DTO
    /// </summary>
    public class OrderStatsResponse
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int InDeliveryOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    /// <summary>
    /// Order query parameters DTO
    /// </summary>
    public class OrderQueryParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1-100")]
        public int PageSize { get; set; } = 10;

        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomerEmail { get; set; }
        public string? VendorEmail { get; set; }
        public string? OrderNumber { get; set; }
    }

    /// <summary>
    /// Order query DTO
    /// </summary>
    public class OrderQueryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1-100")]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomerEmail { get; set; }
        public string? VendorEmail { get; set; }
        public string? OrderNumber { get; set; }
    }

    /// <summary>
    /// Order response DTO
    /// </summary>
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
        public string? DeliveryAddress { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public string? Notes { get; set; }
        public string VendorEmail { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
    }

    /// <summary>
    /// Order item response DTO
    /// </summary>
    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    /// <summary>
    /// Order statistics DTO
    /// </summary>
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int InDeliveryOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    /// <summary>
    /// Update order status DTO
    /// </summary>
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Order status cannot be empty")]
        public string Status { get; set; } = string.Empty;

        public DateTime? EstimatedCompletionTime { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}