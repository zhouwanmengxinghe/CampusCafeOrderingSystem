using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    // UserViewModel - 用于Admin管理用户页面
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

    // 删除重复的 UserProfileViewModel 定义，因为它已经在 ViewModels/UserProfileViewModel.cs 中定义了
    
    // UserCreditsViewModel - 适应 Credits 页面的数据
    public class UserCreditsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public int CurrentCredits { get; set; }
        public int TotalEarned { get; set; }
        public int TotalSpent { get; set; }
        public List<CreditTransaction> CreditHistory { get; set; } = new List<CreditTransaction>();
    }

    // CreditTransaction - 积分交易记录
    public class CreditTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "Earned" or "Spent"
        public int BalanceAfter { get; set; }
    }

    // CreditHistoryItem - 用来显示用户的消费记录
    public class CreditHistoryItem
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Type { get; set; } = string.Empty;        // 添加 Type 字段
    }

    // UserHistoryViewModel - 适应 History 页面的数据
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

    // OrderSummary - 适应订单页面显示
    public class OrderSummary
    {
        public int Id { get; set; }                          // 视图使用的 Id
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public CampusCafeOrderingSystem.Models.OrderStatus Status { get; set; } = CampusCafeOrderingSystem.Models.OrderStatus.Pending; // 改为枚举类型
        public List<string> OrderItems { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
    }

    // FeedbackSummary - 用于历史反馈页面显示
    public class FeedbackSummary
    {
        public int FeedbackId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Subject { get; set; } = string.Empty;
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;  // 使用已有的枚举类型（来自 Models/Feedback.cs）
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? Rating { get; set; }                     // 可空，支持 HasValue
        public string AdminReply { get; set; } = string.Empty;
        public DateTime? RepliedAt { get; set; }
    }
}
