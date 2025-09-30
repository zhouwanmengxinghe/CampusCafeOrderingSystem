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
    
    // 积分相关字段
    [Display(Name = "Use Credits")]
    public bool UseCredits { get; set; } = false;
    
    [Display(Name = "Credits to Use")]
    public decimal CreditsToUse { get; set; } = 0;
    
    public decimal AvailableCredits { get; set; } = 0;
    
    public decimal FinalAmount { get; set; } = 0;
    
    // 配送相关字段
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
        public bool IsSuccessful => IsSuccess; // 为了向后兼容性
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