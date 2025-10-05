using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public class PaymentModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [Display(Name = "Selected Payment Method")]
        public string SelectedPaymentMethod { get; set; } = string.Empty;
        
       
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }
        
        [Display(Name = "Cardholder Name")]
        public string? CardholderName { get; set; }
        
        [Display(Name = "Expiry Date (MM/YY)")]
        public string? ExpiryDate { get; set; }
        
        [Display(Name = "CVV")]
        public string? CVV { get; set; }
        
       
        [Display(Name = "Campus Card Number")]
        public string? CampusCardNumber { get; set; }
        
        [Display(Name = "PIN")]
        public string? CampusCardPin { get; set; }
        
        
        [Display(Name = "Mobile Wallet Type")]
        public string? WalletType { get; set; }
        
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    
    // Points related fields
    [Display(Name = "Use Credits")]
    public bool UseCredits { get; set; } = false;
    
    [Display(Name = "Credits to Use")]
    public decimal CreditsToUse { get; set; } = 0;
    
    public decimal AvailableCredits { get; set; } = 0;
    
    public decimal FinalAmount { get; set; } = 0;
    
    // Delivery related fields
    [Display(Name = "Delivery Type")]
    public DeliveryType DeliveryType { get; set; } = DeliveryType.Pickup;
    
    [Display(Name = "Delivery Address")]
    public string DeliveryAddress { get; set; } = string.Empty;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
    
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string Status { get; set; } = string.Empty;
        // Points related fields
        public int? CreditUsed { get; set; }
        public int? CreditEarned { get; set; }
        public int? CreditBalance { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Delivery related fields
        public string? DeliveryAddress { get; set; }
        public string? DeliveryInstructions { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        
        // Navigation properties
        public Order? Order { get; set; }
        
        // Calculated properties
        public bool IsCompleted => Status == "Completed";
        public bool IsPending => Status == "Pending";
        public bool IsFailed => Status == "Failed";
        public bool IsRefunded => Status == "Refunded";
        
        public bool IsSuccessful => IsSuccess; // For backward compatibility
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionTime { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
    
    public enum PaymentMethodType
    {
        CreditCard,
        CampusCard,
        MobileWallet
    }
}