using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CampusCafeOrderingSystem.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }

        // 新增：便于展示/联系
        public string? UserEmail { get; set; }   // ★ 新增

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public FeedbackCategory Category { get; set; }

        // 评分（保留）
        [Range(1, 5)]
        public int? Rating { get; set; }

        // 新增：优先级
        public FeedbackPriority Priority { get; set; } = FeedbackPriority.Medium; // ★ 新增

        public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;

        public string? AdminResponse { get; set; }
        public string? AdminUserId { get; set; }
        public DateTime? ResponseDate { get; set; }

        public int? OrderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum FeedbackCategory
    {
        General = 0,
        OrderIssue = 1,
        FoodQuality = 2,
        DeliveryService = 3,
        PaymentIssue = 4,
        TechnicalSupport = 5,
        Suggestion = 6,
        Complaint = 7,
        Contact = 8 // 可选：用于“Contact”表单
    }

    public enum FeedbackStatus
    {
        Open = 0,
        InProgress = 1,
        Resolved = 2,
        Closed = 3
    }

    // ★ 新增
    public enum FeedbackPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }
}
