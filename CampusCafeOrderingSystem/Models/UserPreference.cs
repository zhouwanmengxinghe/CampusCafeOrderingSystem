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
        
        // Notification preferences
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;
        public bool OrderStatusNotifications { get; set; } = true;
        public bool PromotionNotifications { get; set; } = false;

        // Dietary preferences
        public string? DietaryRestrictions { get; set; }
        public string? FavoriteCategories { get; set; }
        public string? AllergyInformation { get; set; }

        // Interface preferences
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "zh-CN";

        // Payment preferences
        public string? PreferredPaymentMethod { get; set; }

        // Delivery preferences
        public string? DefaultDeliveryAddress { get; set; }
        public string? DeliveryInstructions { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}