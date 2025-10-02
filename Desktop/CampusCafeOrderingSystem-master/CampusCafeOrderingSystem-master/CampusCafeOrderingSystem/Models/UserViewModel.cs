using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    // UserViewModel - for Admin user management page
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    // Remove duplicate UserProfileViewModel definition, as it's already defined in ViewModels/UserProfileViewModel.cs
    
    // UserCreditsViewModel - adapted for Credits page data
    public class UserCreditsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public int CurrentCredits { get; set; }
        public int TotalEarned { get; set; }
        public int TotalSpent { get; set; }
        public List<CreditTransaction> CreditHistory { get; set; } = new List<CreditTransaction>();
    }

    // CreditTransaction - credit transaction record
    public class CreditTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "Earned" or "Spent"
        public int BalanceAfter { get; set; }
    }

    // CreditHistoryItem - for displaying user consumption records
    public class CreditHistoryItem
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Type { get; set; } = string.Empty;        // Add Type field
    }

    // UserHistoryViewModel - adapted for History page data
    public class UserHistoryViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public List<OrderSummary> RecentOrders { get; set; } = new List<OrderSummary>();
        public List<FeedbackSummary> RecentFeedbacks { get; set; } = new List<FeedbackSummary>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalOrders { get; set; }
        public int TotalPages { get; set; }
    }

    // OrderSummary - adapted for order page display
    public class OrderSummary
    {
        public int Id { get; set; }                          // View uses Id
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public CampusCafeOrderingSystem.Models.OrderStatus Status { get; set; } = CampusCafeOrderingSystem.Models.OrderStatus.Pending; // Changed to enum type
        public List<string> OrderItems { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
    }

    // FeedbackSummary - for historical feedback page display
    public class FeedbackSummary
    {
        public int FeedbackId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Subject { get; set; } = string.Empty;
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;  // Use existing enum type (from Models/Feedback.cs)
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? Rating { get; set; }                     // Nullable, supports HasValue
        public string AdminReply { get; set; } = string.Empty;
        public DateTime? RepliedAt { get; set; }
    }
}
