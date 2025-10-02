using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CampusCafeOrderingSystem.Models
{
    public class UserPreference
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public IdentityUser? User { get; set; }
        
        // 通知偏好
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool OrderStatusNotifications { get; set; } = true;
        public bool PromotionNotifications { get; set; } = true;
        
        // 饮食偏好
        public string? DietaryRestrictions { get; set; }
        public string? FavoriteCategories { get; set; }
        public string? AllergyInformation { get; set; }
        
        // 界面偏好
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "en";
        
        // 支付偏好
        public string PreferredPaymentMethod { get; set; } = "CampusCard";
        
        // 配送偏好
        public string? DefaultDeliveryAddress { get; set; }
        public string? DeliveryInstructions { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}