using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.DTOs
{
    /// <summary>
    /// 创建订单请求DTO
    /// </summary>
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "订单项不能为空")]
        [MinLength(1, ErrorMessage = "至少需要一个订单项")]
        public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();

        [Required(ErrorMessage = "配送方式不能为空")]
        public string DeliveryType { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "配送地址不能超过500个字符")]
        public string? DeliveryAddress { get; set; }

        [Required(ErrorMessage = "支付方式不能为空")]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "备注不能超过1000个字符")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "商家邮箱不能为空")]
        [EmailAddress(ErrorMessage = "商家邮箱格式不正确")]
        public string VendorEmail { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建订单项请求DTO
    /// </summary>
    public class CreateOrderItemRequest
    {
        [Required(ErrorMessage = "菜品ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "菜品ID必须大于0")]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "数量不能为空")]
        [Range(1, 100, ErrorMessage = "数量必须在1-100之间")]
        public int Quantity { get; set; }

        [MaxLength(500, ErrorMessage = "特殊要求不能超过500个字符")]
        public string? SpecialInstructions { get; set; }
    }

    /// <summary>
    /// 更新订单状态请求DTO
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "订单状态不能为空")]
        public string Status { get; set; } = string.Empty;

        public DateTime? EstimatedCompletionTime { get; set; }

        [MaxLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 订单响应DTO
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
    /// 订单项响应DTO
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
    /// 订单统计响应DTO
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
    /// 订单查询参数DTO
    /// </summary>
    public class OrderQueryParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "每页数量必须在1-100之间")]
        public int PageSize { get; set; } = 10;

        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomerEmail { get; set; }
        public string? VendorEmail { get; set; }
        public string? OrderNumber { get; set; }
    }

    /// <summary>
    /// 订单查询DTO
    /// </summary>
    public class OrderQueryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "每页数量必须在1-100之间")]
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
    /// 订单响应DTO
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
    /// 订单项响应DTO
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
    /// 订单统计DTO
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
    /// 更新订单状态DTO
    /// </summary>
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "订单状态不能为空")]
        public string Status { get; set; } = string.Empty;

        public DateTime? EstimatedCompletionTime { get; set; }

        [MaxLength(500, ErrorMessage = "备注不能超过500个字符")]
        public string? Notes { get; set; }
    }
}