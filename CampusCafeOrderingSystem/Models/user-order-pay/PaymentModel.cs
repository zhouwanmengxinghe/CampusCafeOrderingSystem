using System.ComponentModel.DataAnnotations;

namespace CafeApp.Models.user_order_pay
{
    public class PaymentModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public string SelectedPaymentMethod { get; set; } = string.Empty;
        
        // 信用卡支付信息
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }
        
        [Display(Name = "Cardholder Name")]
        public string? CardholderName { get; set; }
        
        [Display(Name = "Expiry Date (MM/YY)")]
        public string? ExpiryDate { get; set; }
        
        [Display(Name = "CVV")]
        public string? CVV { get; set; }
        
        // 校园账户支付信息
        [Display(Name = "Campus Card Number")]
        public string? CampusCardNumber { get; set; }
        
        [Display(Name = "PIN")]
        public string? CampusCardPin { get; set; }
        
        // 移动钱包支付信息
        [Display(Name = "Mobile Wallet Type")]
        public string? WalletType { get; set; }
        
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
    
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
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