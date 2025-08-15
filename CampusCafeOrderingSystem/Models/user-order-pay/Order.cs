using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CafeApp.Models.user_order_pay
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public IdentityUser? User { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [Required]
        public string TransactionId { get; set; } = string.Empty;
        
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        public DateTime? EstimatedCompletionTime { get; set; }
        
        public DateTime? CompletedTime { get; set; }
        
        public string? Notes { get; set; }
        
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        public Order? Order { get; set; }
        
        [Required]
        public int MenuItemId { get; set; }
        
        [Required]
        public string MenuItemName { get; set; } = string.Empty;
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        public decimal TotalPrice => UnitPrice * Quantity;
    }
    
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Preparing = 2,
        Ready = 3,
        Completed = 4,
        Cancelled = 5
    }
}