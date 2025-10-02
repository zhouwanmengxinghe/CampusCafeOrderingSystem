using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CampusCafeOrderingSystem.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Preparing,
        Ready,
        InDelivery,
        Completed,
        Cancelled
    }

    public enum DeliveryType
    {
        Pickup,
        Delivery
    }

    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public IdentityUser? User { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(20)]
        public string? CustomerPhone { get; set; }
        
        [Required]
        [StringLength(100)]
        public string VendorEmail { get; set; } = string.Empty;
        
        public DeliveryType DeliveryType { get; set; } = DeliveryType.Pickup;
        
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }
        
        public DateTime? EstimatedCompletionTime { get; set; }
        
        public DateTime? CompletedTime { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public int MenuItemId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string MenuItemName { get; set; } = string.Empty;
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice => UnitPrice * Quantity;
        
        [StringLength(200)]
        public string? SpecialInstructions { get; set; }
        
        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual MenuItem? MenuItem { get; set; }
    }
}